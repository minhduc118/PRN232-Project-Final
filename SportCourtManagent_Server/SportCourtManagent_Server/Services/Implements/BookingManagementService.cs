using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Booking;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Hubs;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
  public class BookingManagementService : IBookingManagementService
  {
    private readonly IBookingRepository _bookingRepo;
    private readonly IPromotionRepository _promoRepo;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ITournamentLockManager _lockManager;
    private const string PublicTournamentsCacheKey = "PublicTournamentsList";
    private readonly IHubContext<SlotStatusHub> _hubContext;
    private readonly ILogger<BookingManagementService> _logger;

    public BookingManagementService(
      IBookingRepository bookingRepo,
      IPromotionRepository promoRepo,
      AppDbContext context,
      IMemoryCache cache,
      ITournamentLockManager lockManager,
      IHubContext<SlotStatusHub> hubContext,
      ILogger<BookingManagementService> logger)

    {
      _bookingRepo = bookingRepo ?? throw new ArgumentNullException(nameof(bookingRepo));
      _promoRepo = promoRepo ?? throw new ArgumentNullException(nameof(promoRepo));
      _context = context ?? throw new ArgumentNullException(nameof(context));
      _cache = cache ?? throw new ArgumentNullException(nameof(cache));
      _lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
      _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }

    /// <summary>Gets customer bookings asynchronous.</summary>
    public async Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(int userId)
    {
      var bookings = await _bookingRepo.GetCustomerBookingsAsync(userId);
      return bookings.Select(MapToDto).ToList();
    }

    /// <summary>Gets admin bookings with filters asynchronous.</summary>
    public async Task<IEnumerable<BookingDto>> GetAdminBookingsAsync(DateTime? date, int? courtTypeId, string? status)
    {
      var bookings = await _bookingRepo.GetAdminBookingsAsync(date, courtTypeId, status);
      return bookings.Select(MapToDto).ToList();
    }

    /// <summary>Gets booking detail by id asynchronous.</summary>
    public async Task<BookingDto?> GetBookingDetailAsync(int id)
    {
      var booking = await _bookingRepo.GetDetailAsync(id);
      return booking == null ? null : MapToDto(booking);
    }

    /// <summary>Creates a new booking asynchronous.</summary>
    public async Task<BookingDto> CreateBookingAsync(int userId, CreateBookingRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));

      var court = await _context.Courts.FindAsync(request.CourtId);
      if (court == null) throw new ArgumentException("Sân không tồn tại.");
      if (court.IsDeleted)
        throw new InvalidOperationException("Sân đã bị xóa khỏi hệ thống, không thể đặt.");
      if (court.Status == CourtStatus.Inactive || court.Status == CourtStatus.Maintenance)
        throw new InvalidOperationException($"Sân đang ở trạng thái {court.Status}, không thể đặt.");

      var targetSlotIds = (request.SlotIds != null && request.SlotIds.Any())
        ? request.SlotIds.Distinct().OrderBy(s => s).ToList()
        : new List<int> { request.SlotId };

      var slots = await _context.TimeSlots.Where(s => targetSlotIds.Contains(s.SlotId)).OrderBy(s => s.StartTime).ToListAsync();
      if (!slots.Any()) throw new ArgumentException("Khung giờ không hợp lệ.");

      // Validate: reject booking for past date/time
      var nowLocal = DateTime.Now;
      if (request.BookingDate.Date < nowLocal.Date)
        throw new ArgumentException("Ngày đặt sân đã qua, không thể đặt sân.");
      if (request.BookingDate.Date == nowLocal.Date && slots.Min(s => s.StartTime) <= nowLocal.TimeOfDay)
        throw new ArgumentException("Khung giờ đặt sân đã qua thời gian hiện tại, không thể đặt sân.");

      var startTime = slots.Min(s => s.StartTime);
      var endTime = slots.Max(s => s.EndTime);
      var bookingStart = request.BookingDate.Date.Add(startTime);
      var bookingEnd = request.BookingDate.Date.Add(endTime);
      var hasMaintenanceOverlap = await _context.MaintenanceSchedules.AnyAsync(m =>
        m.CourtId == request.CourtId
        && (m.Status == MaintenanceStatus.Scheduled || m.Status == MaintenanceStatus.InProgress)
        && m.StartDateTime < bookingEnd
        && m.EndDateTime > bookingStart);
      if (hasMaintenanceOverlap)
        throw new InvalidOperationException("Khung giờ này nằm trong lịch bảo trì sân, không thể đặt.");

      var primarySlot = slots.First();
      // startTime/endTime already computed above

      var hasPricing = await _context.CourtPricings.AnyAsync(cp => cp.CourtId == request.CourtId && targetSlotIds.Contains(cp.SlotId));
      if (!hasPricing)
      {
          throw new ArgumentException("Sân đấu này không mở cửa hoạt động trong khung giờ đã chọn.");
      }

      var strategy = _context.Database.CreateExecutionStrategy();
      return await strategy.ExecuteAsync(async () =>
      {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
          // Check for duplicate slot booking & apply lazy expiration atomically inside transaction
          var existingBookings = await _context.Bookings.Where(b =>
            b.CourtId == request.CourtId
            && targetSlotIds.Contains(b.SlotId)
            && b.BookingDate.Date == request.BookingDate.Date
            && b.Status != BookingStatus.Cancelled).ToListAsync();

          var now = DateTime.UtcNow;
          foreach (var conflict in existingBookings)
          {
            if (conflict.Status == BookingStatus.Pending && conflict.ExpiredAt.HasValue && conflict.ExpiredAt.Value < now)
            {
              conflict.Status = BookingStatus.Cancelled;
              conflict.CancelReason = "Hết hạn thanh toán (TTL expired)";
              _context.Bookings.Update(conflict);
            }
            else
            {
              throw new ArgumentException($"Khung giờ ngày {request.BookingDate:dd/MM/yyyy} đã có người đặt hoặc đang giữ chỗ chờ thanh toán.");
            }
          }
          if (existingBookings.Any(b => b.Status == BookingStatus.Cancelled))
          {
            await _context.SaveChangesAsync();
          }

          decimal subTotal = 0;
          foreach (var s in slots)
          {
            subTotal += await CalculateSubTotalAsync(request.CourtId, s, null);
          }
          if (request.ServiceIds != null && request.ServiceIds.Any())
          {
            var court = await _context.Courts.FindAsync(request.CourtId);
            if (court != null)
            {
              var serviceIds = request.ServiceIds.Select(s => s.ServiceId).ToList();
              var complexServices = await _context.ComplexCourtTypeServices
                  .Include(cs => cs.Service)
                  .Where(cs => cs.ComplexId == court.ComplexId && cs.CourtTypeId == court.CourtTypeId && serviceIds.Contains(cs.ServiceId))
                  .ToListAsync();

              foreach (var item in request.ServiceIds)
              {
                var match = complexServices.FirstOrDefault(cs => cs.ServiceId == item.ServiceId);
                if (match != null)
                {
                  subTotal += match.Price * item.Quantity;
                }
              }
            }
          }

          var (promoId, discountAmount) = await ProcessPromotionAsync(request.PromotionCode, subTotal);
          decimal totalAmount = Math.Max(0, subTotal - discountAmount);

          // Wallet check & atomic deduction
          var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
          if (wallet == null)
          {
              wallet = new Wallet { UserId = userId, Balance = 10000000m };
              await _context.Wallets.AddAsync(wallet);
              await _context.SaveChangesAsync();
          }

          if (wallet.Balance < totalAmount)
          {
              throw new InvalidOperationException($"Số dư ví không đủ. Chi phí đặt sân là {totalAmount:N0}đ nhưng ví của bạn chỉ còn {wallet.Balance:N0}đ. Vui lòng nạp thêm tiền.");
          }

          wallet.Balance -= totalAmount;
          wallet.UpdatedAt = DateTime.UtcNow;

          var booking = new Booking
          {
            BookingCode = $"BK{DateTime.UtcNow:yyMMddHHmmss}{Guid.NewGuid().ToString("N")[..5].ToUpper()}",
            UserId = userId,
            CourtId = request.CourtId,
            SlotId = primarySlot.SlotId,
            BookingDate = request.BookingDate,
            StartTime = startTime,
            EndTime = endTime,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            Status = BookingStatus.Confirmed,
            PromotionId = promoId,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
          };

          await AddBookingServicesAsync(booking, request.ServiceIds);
          await _bookingRepo.AddAsync(booking);

          var payment = new Payment
          {
              BookingId = booking.BookingId,
              Amount = booking.TotalAmount,
              PaymentMethod = PaymentMethod.Wallet,
              TransactionId = $"WT-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
              Status = PaymentStatus.Success,
              PaidAt = DateTime.UtcNow
          };
          _context.Payments.Add(payment);

          var wt = new WalletTransaction
          {
              WalletId = wallet.WalletId,
              Amount = -booking.TotalAmount,
              Type = WalletTransactionType.Payment,
              BookingId = booking.BookingId,
              Description = $"Thanh toán đặt sân {booking.BookingCode}",
              CreatedAt = DateTime.UtcNow
          };
          await _context.WalletTransactions.AddAsync(wt);
          await _context.SaveChangesAsync();
          await transaction.CommitAsync();

          // Push SignalR slot status update
          foreach (var sId in targetSlotIds)
          {
            await _hubContext.Clients.Group($"court-{request.CourtId}")
              .SendAsync("SlotStatusChanged", request.CourtId, sId, request.BookingDate.ToString("yyyy-MM-dd"), "Booked");
          }

          return MapToDto(await _bookingRepo.GetDetailAsync(booking.BookingId) ?? booking);
        }
        catch
        {
          await transaction.RollbackAsync();
          throw;
        }
      });
    }

    /// <summary>Creates recurring bookings across multiple weeks. Skips conflicting dates (Option A).</summary>
    public async Task<RecurringBookingResponseDto> CreateRecurringBookingAsync(int userId, CreateRecurringBookingRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
      var nowLocal = DateTime.Now;
      if (request.StartDate.Date < nowLocal.Date)
        throw new ArgumentException("Ngày bắt đầu đặt sân không được ở trong quá khứ.");
      if (request.DaysOfWeek == null || !request.DaysOfWeek.Any())
        throw new ArgumentException("Phải chọn ít nhất một ngày trong tuần.");
      if (request.EndDate <= request.StartDate)
        throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");
      if ((request.EndDate - request.StartDate).TotalDays > 365)
        throw new ArgumentException("Khoảng thời gian đặt định kỳ tối đa là 1 năm.");

      var slot = await _context.TimeSlots.FindAsync(request.SlotId);
      if (slot == null) throw new ArgumentException("Khung giờ không hợp lệ.");

      var court = await _context.Courts.FindAsync(request.CourtId);
      if (court == null) throw new ArgumentException("Sân không tồn tại.");
      if (court.IsDeleted)
        throw new InvalidOperationException("Sân đã bị xóa khỏi hệ thống, không thể đặt.");
      if (court.Status == CourtStatus.Inactive || court.Status == CourtStatus.Maintenance)
        throw new InvalidOperationException($"Sân đang ở trạng thái {court.Status}, không thể đặt định kỳ.");

      var targetSlotIds = (request.SlotIds != null && request.SlotIds.Any())
        ? request.SlotIds.Distinct().OrderBy(s => s).ToList()
        : new List<int> { request.SlotId };

      var slots = await _context.TimeSlots.Where(s => targetSlotIds.Contains(s.SlotId)).OrderBy(s => s.StartTime).ToListAsync();
      if (!slots.Any()) throw new ArgumentException("Khung giờ không hợp lệ.");

      var primarySlot = slots.First();
      var startTime = slots.Min(s => s.StartTime);
      var endTime = slots.Max(s => s.EndTime);

      // Chặn nếu có bảo trì overlap bất kỳ ngày nào trong range (kiểm tra thô theo range)
      var rangeStart = request.StartDate.Date.Add(startTime);
      var rangeEnd = request.EndDate.Date.Add(endTime);
      var hasMaintenanceOverlap = await _context.MaintenanceSchedules.AnyAsync(m =>
        m.CourtId == request.CourtId
        && (m.Status == MaintenanceStatus.Scheduled || m.Status == MaintenanceStatus.InProgress)
        && m.StartDateTime < rangeEnd
        && m.EndDateTime > rangeStart);
      if (hasMaintenanceOverlap)
        throw new InvalidOperationException("Khoảng đặt định kỳ giao với lịch bảo trì sân. Vui lòng chọn thời gian khác hoặc đợi hết bảo trì.");

      // Check if court supports these slots
      var hasPricing = await _context.CourtPricings.AnyAsync(cp => cp.CourtId == request.CourtId && targetSlotIds.Contains(cp.SlotId));
      if (!hasPricing)
      {
          throw new ArgumentException("Sân đấu này không mở cửa hoạt động trong khung giờ đã chọn.");
      }

      // Validate that selected DaysOfWeek actually occur in the date range
      var validDaysInRange = new HashSet<int>();
      for (var d = request.StartDate.Date; d <= request.EndDate.Date; d = d.AddDays(1))
      {
        validDaysInRange.Add((int)d.DayOfWeek);
      }

      var invalidDays = request.DaysOfWeek.Where(dow => !validDaysInRange.Contains(dow)).ToList();
      if (invalidDays.Any())
      {
        string invalidNames = string.Join(", ", invalidDays.Select(d => d switch {
          0 => "Chủ Nhật", 1 => "Thứ 2", 2 => "Thứ 3", 3 => "Thứ 4", 4 => "Thứ 5", 5 => "Thứ 6", 6 => "Thứ 7", _ => d.ToString()
        }));
        throw new ArgumentException($"Các ngày ({invalidNames}) không xuất hiện trong khoảng thời gian từ {request.StartDate:dd/MM/yyyy} đến {request.EndDate:dd/MM/yyyy}.");
      }

      // Generate all target dates from StartDate to EndDate matching DaysOfWeek
      // Also filter out past date/time slots
      var allDates = new List<DateTime>();
      for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
      {
        if (request.DaysOfWeek.Contains((int)date.DayOfWeek))
        {
          // Skip past dates entirely
          if (date < nowLocal.Date) continue;
          // For today, skip if slot start time has already passed
          if (date == nowLocal.Date && startTime <= nowLocal.TimeOfDay) continue;
          allDates.Add(date);
        }
      }

      if (!allDates.Any())
        throw new ArgumentException("Tất cả các ngày trong khoảng thời gian đã chọn đều đã qua hoặc đã có người đặt.");

      // Batch check all conflicting dates & apply lazy expiration
      var existingBookings = await _context.Bookings
        .Where(b => b.CourtId == request.CourtId
                  && targetSlotIds.Contains(b.SlotId)
                  && allDates.Contains(b.BookingDate)
                  && b.Status != BookingStatus.Cancelled)
        .ToListAsync();

      var now = DateTime.UtcNow;
      var activeConflictDates = new HashSet<DateTime>();
      foreach (var conflict in existingBookings)
      {
        if (conflict.Status == BookingStatus.Pending && conflict.ExpiredAt.HasValue && conflict.ExpiredAt.Value < now)
        {
          conflict.Status = BookingStatus.Cancelled;
          conflict.CancelReason = "Hết hạn thanh toán (TTL expired)";
          _context.Bookings.Update(conflict);
        }
        else
        {
          activeConflictDates.Add(conflict.BookingDate.Date);
        }
      }
      if (existingBookings.Any(b => b.Status == BookingStatus.Cancelled))
      {
        await _context.SaveChangesAsync();
      }

      var conflictDates = allDates.Where(d => activeConflictDates.Contains(d.Date)).ToList();
      var availableDates = allDates.Where(d => !activeConflictDates.Contains(d.Date)).ToList();

      if (!availableDates.Any())
        throw new ArgumentException("Tất cả các ngày trong khoảng thời gian đã chọn đều đã có người đặt.");

      var strategy = _context.Database.CreateExecutionStrategy();
      return await strategy.ExecuteAsync(async () =>
      {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
          // Create RecurringBooking parent record
          var daysStr = string.Join(",", request.DaysOfWeek.OrderBy(d => d));
          var recurring = new RecurringBooking
          {
            UserId = userId,
            CourtId = request.CourtId,
            SlotId = primarySlot.SlotId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DaysOfWeek = daysStr,
            Status = RecurringBookingStatus.Active
          };
          await _context.RecurringBookings.AddAsync(recurring);
          await _context.SaveChangesAsync();

          // Create individual bookings for each available date (Status = Pending, 5-minute TTL)
          decimal totalAmount = 0;
          var createdBookings = new List<Booking>();

          foreach (var date in availableDates)
          {
            decimal daySubTotal = 0;
            foreach (var s in slots)
            {
              daySubTotal += await CalculateSubTotalAsync(request.CourtId, s, null);
            }

            var booking = new Booking
            {
              BookingCode = $"RBK{DateTime.UtcNow:yyMMddHHmmss}{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
              UserId = userId,
              CourtId = request.CourtId,
              SlotId = primarySlot.SlotId,
              BookingDate = date,
              StartTime = startTime,
              EndTime = endTime,
              SubTotal = daySubTotal,
              DiscountAmount = 0,
              TotalAmount = daySubTotal,
              Status = BookingStatus.Confirmed,
              Note = request.Note,
              CreatedAt = DateTime.UtcNow,
              ExpiredAt = null
            };

            totalAmount += daySubTotal;
            await _context.Bookings.AddAsync(booking);
            createdBookings.Add(booking);
          }

          // Apply promotion if provided
          decimal discountAmount = 0;
          int? promoId = null;
          if (!string.IsNullOrWhiteSpace(request.PromotionCode))
          {
            var promoResult = await ProcessPromotionAsync(request.PromotionCode, totalAmount);
            promoId = promoResult.promoId;
            discountAmount = promoResult.discount;

            if (promoId.HasValue && createdBookings.Any())
            {
              decimal perBookingDiscount = discountAmount / createdBookings.Count;
              foreach (var b in createdBookings)
              {
                b.PromotionId = promoId;
                b.DiscountAmount = perBookingDiscount;
                b.TotalAmount = Math.Max(0, b.SubTotal - perBookingDiscount);
              }
            }
          }

          // Deduct wallet balance
          var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
          if (wallet == null)
          {
              wallet = new Wallet { UserId = userId, Balance = 10000000m };
              await _context.Wallets.AddAsync(wallet);
              await _context.SaveChangesAsync();
          }

          decimal netTotalAmount = Math.Max(0, totalAmount - discountAmount);
          if (wallet.Balance < netTotalAmount)
          {
              throw new InvalidOperationException($"Số dư ví không đủ. Chi phí đặt lịch định kỳ là {netTotalAmount:N0}đ nhưng ví của bạn chỉ còn {wallet.Balance:N0}đ. Vui lòng nạp thêm tiền.");
          }

          wallet.Balance -= netTotalAmount;
          wallet.UpdatedAt = DateTime.UtcNow;

          var wt = new WalletTransaction
          {
              WalletId = wallet.WalletId,
              Amount = -netTotalAmount,
              Type = WalletTransactionType.Payment,
              Description = $"Thanh toán đặt lịch định kỳ sân #{request.CourtId}",
              CreatedAt = DateTime.UtcNow
          };
          await _context.WalletTransactions.AddAsync(wt);

          await _context.SaveChangesAsync();
          await transaction.CommitAsync();

          // Push SignalR updates for all booked slots
          foreach (var date in availableDates)
          {
            await _hubContext.Clients.Group($"court-{request.CourtId}")
              .SendAsync("SlotStatusChanged", request.CourtId, request.SlotId, date.ToString("yyyy-MM-dd"), "Booked");
          }

          // Map day numbers to Vietnamese day names
          string daysDisplay = string.Join(", ", request.DaysOfWeek.OrderBy(d => d).Select(d => d switch
          {
            0 => "CN",
            1 => "T2",
            2 => "T3",
            3 => "T4",
            4 => "T5",
            5 => "T6",
            6 => "T7",
            _ => d.ToString()
          }));

          string responseSlotName = slots.Count > 1
            ? $"{CleanSlotName(slots.First().SlotName)} - {CleanSlotName(slots.Last().SlotName)}"
            : slots.First().SlotName;

          return new RecurringBookingResponseDto
          {
            RecurringId = recurring.RecurringId,
            CourtId = court.CourtId,
            CourtName = court.CourtName,
            SlotId = primarySlot.SlotId,
            SlotName = responseSlotName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DaysOfWeek = daysDisplay,
            Status = recurring.Status.ToString(),
            CreatedBookings = createdBookings.Select(MapToDto).ToList(),
            ConflictDates = conflictDates.Select(d => d.ToString("dd/MM/yyyy")).ToList(),
            TotalRequestedSessions = allDates.Count,
            TotalBookedSessions = availableDates.Count,
            TotalEstimatedAmount = Math.Max(0, totalAmount - discountAmount)
          };
        }
        catch (Exception)
        {
          await transaction.RollbackAsync();
          throw;
        }
      });
    }

    /// <summary>Updates booking status asynchronous.</summary>
    public async Task<BookingDto?> UpdateBookingStatusAsync(int id, UpdateBookingStatusRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));

      var booking = await _bookingRepo.GetDetailAsync(id);
      if (booking == null) return null;

      if (request.Status == BookingStatus.Cancelled && booking.Status != BookingStatus.Cancelled)
      {
        if ((booking.Status == BookingStatus.Confirmed || (booking.Payment != null && booking.Payment.Status == PaymentStatus.Success))
            && string.IsNullOrWhiteSpace(request.CancelReason))
        {
          throw new ArgumentException("Vui lòng nhập lý do hủy đơn đã xác nhận/thanh toán để phục vụ đối soát hoàn tiền.");
        }
        booking.Status = BookingStatus.Cancelled;
        booking.CancelReason = request.CancelReason ?? "Đơn bị hủy bởi Admin.";
        await HandleCancellationAsync(booking);
      }
      else
      {
        booking.Status = request.Status;
        if (request.CancelReason != null) booking.CancelReason = request.CancelReason;
      }

      await _bookingRepo.UpdateAsync(booking);
      return MapToDto(booking);
    }

    /// <summary>Creates a new tournament booking asynchronous with concurrency lock and strict batching.</summary>
    public async Task<TournamentDto> CreateTournamentAsync(int userId, CreateTournamentRequest request)
    {
      ValidateCreateTournamentRequest(request);
      var lockPairs = GetUniqueCourtDatePairs(request);
      var acquiredPairs = new List<(int CourtId, DateTime Date)>();

      try
      {
        await AcquireTournamentLocksAsync(lockPairs, acquiredPairs);
        var tournament = await ExecuteCreateTournamentTransactionAsync(userId, request);
        await BroadcastTournamentSlotStatusesAsync(tournament.Bookings, "Held");
        return tournament;
      }
      finally
      {
        ReleaseTournamentLocks(acquiredPairs);
      }
    }

    /// <summary>Validates the tournament request, including past-time check.</summary>
    private void ValidateCreateTournamentRequest(CreateTournamentRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
      if (request.CourtSelections == null || !request.CourtSelections.Any())
        throw new ArgumentException("Giải đấu phải có ít nhất một sân được chọn.");

      // Validate: reject tournament bookings for past date/time
      var nowLocal = DateTime.Now;
      foreach (var sel in request.CourtSelections)
      {
        if (sel.BookingDate.Date < nowLocal.Date)
          throw new ArgumentException($"Ngày {sel.BookingDate:dd/MM/yyyy} đã qua, không thể tạo giải đấu.");

        if (sel.BookingDate.Date == nowLocal.Date && sel.SlotIds != null && sel.SlotIds.Any())
        {
          var selSlots = _context.TimeSlots.Where(s => sel.SlotIds.Contains(s.SlotId)).ToList();
          if (selSlots.Any() && selSlots.Min(s => s.StartTime) <= nowLocal.TimeOfDay)
            throw new ArgumentException($"Khung giờ đã chọn cho ngày {sel.BookingDate:dd/MM/yyyy} đã qua thời gian hiện tại, không thể tạo giải đấu.");
        }
      }
    }

    /// <summary>Gets unique court and date pairs sorted to prevent deadlock.</summary>
    private static List<(int CourtId, DateTime Date)> GetUniqueCourtDatePairs(CreateTournamentRequest request)
    {
      return request.CourtSelections
        .Select(c => (c.CourtId, c.BookingDate.Date))
        .Distinct()
        .OrderBy(x => x.CourtId).ThenBy(x => x.Date)
        .ToList();
    }

    /// <summary>Acquires in-memory semaphore locks for courts and dates.</summary>
    private async Task AcquireTournamentLocksAsync(List<(int CourtId, DateTime Date)> pairs, List<(int CourtId, DateTime Date)> acquired)
    {
      foreach (var pair in pairs)
      {
        var ok = await _lockManager.AcquireLockAsync(pair.CourtId, pair.Date, TimeSpan.FromSeconds(10));
        if (!ok) throw new ArgumentException($"Hệ thống đang bận xử lý sân {pair.CourtId}. Vui lòng thử lại sau giây lát.");
        acquired.Add(pair);
      }
    }

    /// <summary>Releases acquired tournament locks.</summary>
    private void ReleaseTournamentLocks(List<(int CourtId, DateTime Date)> acquired)
    {
      foreach (var pair in acquired)
      {
        _lockManager.ReleaseLock(pair.CourtId, pair.Date);
      }
    }

    /// <summary>Broadcasts tournament slot changes without allowing a transient hub failure to fail a committed booking.</summary>
    private async Task BroadcastTournamentSlotStatusesAsync(IEnumerable<BookingDto> bookings, string status)
    {
      try
      {
        foreach (var booking in bookings)
        {
          await _hubContext.Clients.Group($"court-{booking.CourtId}")
            .SendAsync("SlotStatusChanged", booking.CourtId, booking.SlotId, booking.BookingDate.ToString("yyyy-MM-dd"), status);
        }
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Could not broadcast tournament slot status {Status}.", status);
      }
    }

    /// <summary>Creates a tournament booking with immediate wallet payment in a single atomic transaction.</summary>
    public async Task<TournamentDto> CreateAndPayTournamentWithWalletAsync(int userId, CreateTournamentRequest request)
    {
      var strategy = _context.Database.CreateExecutionStrategy();
      return await strategy.ExecuteAsync(async () =>
      {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
          var tournament = new Tournament
          {
            TournamentName = request.TournamentName,
            Description = request.Description,
            UserId = userId,
            Status = TournamentStatus.Paid,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = null
          };
          await _context.Tournaments.AddAsync(tournament);
          await _context.SaveChangesAsync();

          var createdBookings = await ProcessTournamentBookingsAsync(userId, tournament.TournamentId, request);
          decimal totalAmount = createdBookings.Sum(b => b.SubTotal);
          await ApplyTournamentPromotionAsync(tournament, createdBookings, request.PromotionCode, totalAmount);
          await _context.SaveChangesAsync();

          // Deduct wallet
          var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
          if (wallet == null)
          {
              wallet = new Wallet { UserId = userId, Balance = 10000000m };
              await _context.Wallets.AddAsync(wallet);
              await _context.SaveChangesAsync();
          }

          if (wallet.Balance < tournament.TotalAmount)
          {
              throw new InvalidOperationException($"Số dư ví không đủ. Chi phí giải đấu là {tournament.TotalAmount:N0}đ nhưng ví của bạn chỉ còn {wallet.Balance:N0}đ. Vui lòng nạp thêm tiền.");
          }

          wallet.Balance -= tournament.TotalAmount;
          wallet.UpdatedAt = DateTime.UtcNow;

          var wt = new WalletTransaction
          {
              WalletId = wallet.WalletId,
              Amount = -tournament.TotalAmount,
              Type = WalletTransactionType.Payment,
              Description = $"Thanh toán giải đấu {tournament.TournamentName}",
              CreatedAt = DateTime.UtcNow
          };
          await _context.WalletTransactions.AddAsync(wt);
          await _context.SaveChangesAsync();

          await transaction.CommitAsync();
          _cache.Remove(PublicTournamentsCacheKey);

          var user = await _context.Users.FindAsync(userId);
          return MapToTournamentDtoWithCustomer(tournament, user?.FullName ?? $"User #{userId}", createdBookings);
        }
        catch
        {
          await transaction.RollbackAsync();
          throw;
        }
      });
    }

    /// <summary>Executes tournament creation inside a database transaction.</summary>
    private async Task<TournamentDto> ExecuteCreateTournamentTransactionAsync(int userId, CreateTournamentRequest request)
    {
      var strategy = _context.Database.CreateExecutionStrategy();
      return await strategy.ExecuteAsync(async () =>
      {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
          var tournament = new Tournament
          {
            TournamentName = request.TournamentName,
            Description = request.Description,
            UserId = userId,
            Status = TournamentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = null
          };
          await _context.Tournaments.AddAsync(tournament);
          await _context.SaveChangesAsync();

          var createdBookings = await ProcessTournamentBookingsAsync(userId, tournament.TournamentId, request);
          decimal totalAmount = createdBookings.Sum(b => b.SubTotal);
          await ApplyTournamentPromotionAsync(tournament, createdBookings, request.PromotionCode, totalAmount);

          await _context.SaveChangesAsync();
          await transaction.CommitAsync();
          _cache.Remove(PublicTournamentsCacheKey);

          var user = await _context.Users.FindAsync(userId);
          return MapToTournamentDtoWithCustomer(tournament, user?.FullName ?? $"User #{userId}", createdBookings);
        }
        catch
        {
          await transaction.RollbackAsync();
          throw;
        }
      });

    }

    /// <summary>Processes batch loading and creates tournament bookings without loop queries.</summary>
    private async Task<List<Booking>> ProcessTournamentBookingsAsync(int userId, int tournamentId, CreateTournamentRequest request)
    {
      var courtIds = request.CourtSelections.Select(c => c.CourtId).Distinct().ToList();
      var slotIds = request.CourtSelections.SelectMany(c => c.SlotIds ?? new List<int>()).Distinct().ToList();
      var serviceIds = request.CourtSelections.Where(c => c.Services != null).SelectMany(c => c.Services!).Select(s => s.ServiceId).Distinct().ToList();

      var slots = await _context.TimeSlots.Where(s => slotIds.Contains(s.SlotId)).ToDictionaryAsync(s => s.SlotId);
      var courts = await _context.Courts.Where(c => courtIds.Contains(c.CourtId)).ToDictionaryAsync(c => c.CourtId);
      var pricings = await _context.CourtPricings.Where(p => courtIds.Contains(p.CourtId) && slotIds.Contains(p.SlotId)).ToListAsync();
      
      var complexIds = courts.Values.Select(c => c.ComplexId).Distinct().ToList();
      var courtTypeIds = courts.Values.Select(c => c.CourtTypeId).Distinct().ToList();
      var complexServices = await _context.ComplexCourtTypeServices
          .Include(cs => cs.Service)
          .Where(cs => complexIds.Contains(cs.ComplexId) && courtTypeIds.Contains(cs.CourtTypeId) && serviceIds.Contains(cs.ServiceId))
          .ToListAsync();

      var dates = request.CourtSelections.Select(c => c.BookingDate.Date).Distinct().ToList();
      var activeBookings = await _context.Bookings.Where(b => courtIds.Contains(b.CourtId) && dates.Contains(b.BookingDate.Date) && b.Status != BookingStatus.Cancelled).ToListAsync();

      var createdBookings = new List<Booking>();
      foreach (var sel in request.CourtSelections)
      {
        if (sel.SlotIds == null || !sel.SlotIds.Any()) continue;
        var selSlots = sel.SlotIds.Where(sId => slots.ContainsKey(sId)).Select(sId => slots[sId]).OrderBy(s => s.StartTime).ToList();
        if (!selSlots.Any()) continue;

        var primarySlot = selSlots.First();
        var startTime = selSlots.Min(s => s.StartTime);
        var endTime = selSlots.Max(s => s.EndTime);

        foreach (var sId in sel.SlotIds)
        {
          if (slots.TryGetValue(sId, out var sl))
          {
            await CheckSlotConflictAndLazyExpireAsync(sel.CourtId, sId, sel.BookingDate, sl.SlotName, activeBookings);
          }
        }

        decimal selSubTotal = 0;
        for (int i = 0; i < selSlots.Count; i++)
        {
          var s = selSlots[i];
          var reqServices = (i == 0) ? sel.Services : null;
          selSubTotal += CalculateBatchSubTotal(sel.CourtId, s, reqServices, courts, pricings, complexServices);
        }

        var booking = new Booking
        {
          BookingCode = $"BK{DateTime.UtcNow:yyMMddHHmmss}{Guid.NewGuid().ToString("N")[..5].ToUpper()}",
          UserId = userId,
          CourtId = sel.CourtId,
          SlotId = primarySlot.SlotId,
          BookingDate = sel.BookingDate,
          StartTime = startTime,
          EndTime = endTime,
          SubTotal = selSubTotal,
          TotalAmount = selSubTotal,
          Status = BookingStatus.Confirmed,
          TournamentId = tournamentId,
          Note = request.Note,
          CreatedAt = DateTime.UtcNow,
          ExpiredAt = null
        };
        await AddBookingServicesAsync(booking, sel.Services);

        activeBookings.Add(booking);
        createdBookings.Add(booking);
      }
      await _context.Bookings.AddRangeAsync(createdBookings);
      return createdBookings;
    }

    /// <summary>Builds a single booking entity and checks lazy expiration.</summary>
    private async Task<Booking> BuildSingleTournamentBookingAsync(
      int userId, int tournamentId, int courtId, int slotId, DateTime bookingDate, List<ServiceItemRequest>? reqServices, string? note,
      Dictionary<int, TimeSlot> slots, Dictionary<int, Court> courts, List<CourtPricing> pricings,
      List<ComplexCourtTypeService> complexServices, List<Booking> activeBookings)
    {
      if (!slots.TryGetValue(slotId, out var slot)) throw new ArgumentException($"Khung giờ {slotId} không hợp lệ.");
      await CheckSlotConflictAndLazyExpireAsync(courtId, slotId, bookingDate, slot.SlotName, activeBookings);

      decimal subTotal = CalculateBatchSubTotal(courtId, slot, reqServices, courts, pricings, complexServices);
      var booking = new Booking
      {
        BookingCode = $"BK{DateTime.UtcNow:yyMMddHHmmss}{Guid.NewGuid().ToString("N")[..5].ToUpper()}",
        UserId = userId,
        CourtId = courtId,
        SlotId = slotId,
        BookingDate = bookingDate,
        StartTime = slot.StartTime,
        EndTime = slot.EndTime,
        SubTotal = subTotal,
        TotalAmount = subTotal,
        Status = BookingStatus.Confirmed,
        TournamentId = tournamentId,
        Note = note,
        CreatedAt = DateTime.UtcNow,
        ExpiredAt = null
      };
      AddBatchBookingServices(booking, reqServices, courts, complexServices);
      return booking;
    }

    /// <summary>Checks slot conflict and applies lazy expiration for pending timed-out bookings.</summary>
    private async Task CheckSlotConflictAndLazyExpireAsync(int courtId, int slotId, DateTime date, string slotName, List<Booking> activeBookings)
    {
      var conflict = activeBookings.FirstOrDefault(b => b.CourtId == courtId && b.SlotId == slotId && b.BookingDate.Date == date.Date && b.Status != BookingStatus.Cancelled);
      if (conflict != null)
      {
        if (conflict.Status == BookingStatus.Pending && conflict.ExpiredAt.HasValue && conflict.ExpiredAt.Value < DateTime.UtcNow)
        {
          conflict.Status = BookingStatus.Cancelled;
          conflict.CancelReason = "Hết hạn thanh toán (TTL expired)";
          _context.Bookings.Update(conflict);
          await _context.SaveChangesAsync();
          activeBookings.Remove(conflict);
        }
        else
        {
          throw new ArgumentException($"Sân {courtId} vào khung giờ {slotName} ngày {date:dd/MM/yyyy} đã có người đặt hoặc đang giữ chỗ thanh toán.");
        }
      }
    }

    /// <summary>Calculates subtotal using batch loaded dictionaries.</summary>
    private static decimal CalculateBatchSubTotal(int courtId, TimeSlot slot, List<ServiceItemRequest>? reqServices, Dictionary<int, Court> courts, List<CourtPricing> pricings, List<ComplexCourtTypeService> complexServices)
    {
      var pricing = pricings.FirstOrDefault(p => p.CourtId == courtId && p.SlotId == slot.SlotId);
      decimal subTotal = pricing != null ? pricing.Price : 0;
      if (subTotal == 0)
      {
        var anyPricing = pricings.FirstOrDefault(p => p.CourtId == courtId && p.Price > 0);
        if (anyPricing != null)
        {
          subTotal = anyPricing.Price;
        }
        else if (courts.TryGetValue(courtId, out var c))
        {
          decimal hours = (decimal)(slot.EndTime - slot.StartTime).TotalHours;
          subTotal = (c.PricePerHour > 0 ? c.PricePerHour : 100000m) * (hours > 0 ? hours : 1);
        }
      }

      if (reqServices != null && courts.TryGetValue(courtId, out var crt))
      {
        foreach (var item in reqServices.Where(x => x.Quantity > 0))
        {
          var s = complexServices.FirstOrDefault(cs => cs.ComplexId == crt.ComplexId && cs.CourtTypeId == crt.CourtTypeId && cs.ServiceId == item.ServiceId);
          decimal price = s != null ? (s.Price > 0 ? s.Price : (s.Service?.Price ?? 0)) : 0;
          subTotal += price * item.Quantity;
        }
      }
      return subTotal;
    }

    /// <summary>Adds booking services from batch dictionary.</summary>
    private static void AddBatchBookingServices(Booking booking, List<ServiceItemRequest>? reqServices, Dictionary<int, Court> courts, List<ComplexCourtTypeService> complexServices)
    {
      if (reqServices == null || !courts.TryGetValue(booking.CourtId, out var crt)) return;
      foreach (var item in reqServices.Where(x => x.Quantity > 0))
      {
        var s = complexServices.FirstOrDefault(cs => cs.ComplexId == crt.ComplexId && cs.CourtTypeId == crt.CourtTypeId && cs.ServiceId == item.ServiceId);
        decimal price = s != null ? (s.Price > 0 ? s.Price : (s.Service?.Price ?? 0)) : 0;
        if (price > 0)
        {
          booking.BookingServices.Add(new BookingService { ServiceId = item.ServiceId, Quantity = item.Quantity, TotalPrice = price * item.Quantity });
        }
      }
    }

    /// <summary>Applies promotion discount across tournament bookings.</summary>
    private async Task ApplyTournamentPromotionAsync(Tournament tournament, List<Booking> bookings, string? promoCode, decimal totalAmount)
    {
      if (string.IsNullOrWhiteSpace(promoCode))
      {
        tournament.TotalAmount = totalAmount;
        return;
      }
      var (promoId, discount) = await ProcessPromotionAsync(promoCode, totalAmount);
      if (promoId.HasValue && bookings.Any())
      {
        decimal perBookingDiscount = discount / bookings.Count;
        foreach (var b in bookings)
        {
          b.PromotionId = promoId;
          b.DiscountAmount = perBookingDiscount;
          b.TotalAmount = Math.Max(0, b.SubTotal - perBookingDiscount);
        }
      }
      tournament.TotalAmount = Math.Max(0, totalAmount - discount);
    }

    /// <summary>Updates tournament info using smart diffing without deleting old records.</summary>
    public async Task<TournamentDto?> UpdateTournamentInfoAsync(int tournamentId, int userId, UpdateTournamentInfoRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
      if (request.CourtSelections == null || !request.CourtSelections.Any())
        throw new ArgumentException("Giải đấu phải có ít nhất một sân được chọn.");

      var tournament = await _context.Tournaments.Include(t => t.User).Include(t => t.Bookings).ThenInclude(b => b.BookingServices).FirstOrDefaultAsync(t => t.TournamentId == tournamentId);
      if (tournament == null) return null;
      ValidateTournamentEditPermissions(tournament, userId);

      var strategy = _context.Database.CreateExecutionStrategy();
      return await strategy.ExecuteAsync(async () =>
      {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
          tournament.TournamentName = request.TournamentName;
          tournament.Description = request.Description;
          await ApplyTournamentDiffingAsync(tournament, userId, request);

          await _context.SaveChangesAsync();
          await transaction.CommitAsync();
          _cache.Remove(PublicTournamentsCacheKey);
          return await GetTournamentDetailAsync(tournamentId, userId, false);
        }
        catch
        {
          await transaction.RollbackAsync();
          throw;
        }
      });

    }

    /// <summary>Validates edit permissions for tournament.</summary>
    private static void ValidateTournamentEditPermissions(Tournament tournament, int userId)
    {
      if (tournament.UserId != userId) throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa giải đấu này.");
      if (tournament.Status == TournamentStatus.Cancelled || tournament.Status == TournamentStatus.Paid || tournament.Status == TournamentStatus.Confirmed)
        throw new ArgumentException("Không thể chỉnh sửa giải đấu đã xác nhận/thanh toán hoặc đã hủy.");
      if (DateTime.UtcNow - tournament.CreatedAt > TimeSpan.FromHours(24))
        throw new ArgumentException("Chỉ được chỉnh sửa trong vòng 24 giờ kể từ khi tạo giải đấu.");
    }

    /// <summary>Applies smart diffing for tournament bookings. Assumes transaction is active.</summary>
    private async Task ApplyTournamentDiffingAsync(Tournament tournament, int userId, UpdateTournamentInfoRequest request)
    {
      var requestedPairs = new HashSet<(int CourtId, int SlotId, DateTime Date)>();
      var courtSlotServices = new Dictionary<(int, int, DateTime), List<ServiceItemRequest>?>();
      
      foreach (var sel in request.CourtSelections)
      {
        if (sel.SlotIds == null) continue;
        for (int i = 0; i < sel.SlotIds.Count; i++)
        {
           var slotId = sel.SlotIds[i];
           requestedPairs.Add((sel.CourtId, slotId, sel.BookingDate.Date));
           courtSlotServices[(sel.CourtId, slotId, sel.BookingDate.Date)] = (i == 0) ? sel.Services : null;
        }
      }

      foreach (var existing in tournament.Bookings)
      {
        if (!requestedPairs.Contains((existing.CourtId, existing.SlotId, existing.BookingDate.Date)) && existing.Status != BookingStatus.Cancelled)
        {
          existing.Status = BookingStatus.Cancelled;
          existing.CancelReason = "Bỏ ca khi sửa giải đấu";
        }
      }

      var existingActivePairs = new HashSet<(int CourtId, int SlotId, DateTime Date)>(
        tournament.Bookings.Where(b => b.Status != BookingStatus.Cancelled).Select(b => (b.CourtId, b.SlotId, b.BookingDate.Date)));

      var newBookings = new List<Booking>();
      foreach (var pair in requestedPairs)
      {
        if (!existingActivePairs.Contains(pair))
        {
          var existingConflicts = await _context.Bookings.Where(b =>
            b.CourtId == pair.CourtId
            && b.SlotId == pair.SlotId
            && b.BookingDate.Date == pair.Date
            && b.Status != BookingStatus.Cancelled).ToListAsync();

          var now = DateTime.UtcNow;
          foreach (var conflict in existingConflicts)
          {
            if (conflict.Status == BookingStatus.Pending && conflict.ExpiredAt.HasValue && conflict.ExpiredAt.Value < now)
            {
              conflict.Status = BookingStatus.Cancelled;
              conflict.CancelReason = "Hết hạn thanh toán (TTL expired)";
              _context.Bookings.Update(conflict);
            }
            else
            {
              throw new ArgumentException($"Sân {pair.CourtId} vào khung giờ {pair.SlotId} ngày {pair.Date:dd/MM/yyyy} đã có người đặt.");
            }
          }

          var slot = await _context.TimeSlots.FindAsync(pair.SlotId) ?? throw new ArgumentException($"Khung giờ {pair.SlotId} không hợp lệ.");
          var reqServices = courtSlotServices.TryGetValue(pair, out var srv) ? srv : null;
          decimal subTotal = await CalculateSubTotalAsync(pair.CourtId, slot, reqServices);
          var booking = new Booking
          {
            BookingCode = $"TBK{DateTime.UtcNow:yyMMddHHmmss}{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
            UserId = userId, CourtId = pair.CourtId, SlotId = pair.SlotId, BookingDate = pair.Date,
            StartTime = slot.StartTime, EndTime = slot.EndTime, SubTotal = subTotal, TotalAmount = subTotal,
            Status = BookingStatus.Confirmed, TournamentId = tournament.TournamentId, Note = request.Note,
            CreatedAt = DateTime.UtcNow, ExpiredAt = null
          };
          await AddBookingServicesAsync(booking, reqServices);
          newBookings.Add(booking);
        }
      }
      await _context.Bookings.AddRangeAsync(newBookings);
      decimal totalAmount = tournament.Bookings.Where(b => b.Status != BookingStatus.Cancelled).Sum(b => b.SubTotal) + newBookings.Sum(b => b.SubTotal);
      var (promoId, discount) = await ProcessPromotionAsync(request.PromotionCode, totalAmount);
      tournament.TotalAmount = Math.Max(0, totalAmount - discount);
    }

    /// <summary>Calculates subtotal from court pricing and services.</summary>
    private async Task<decimal> CalculateSubTotalAsync(int courtId, TimeSlot slot, List<ServiceItemRequest>? services)
    {
      var pricing = await _context.CourtPricings.FirstOrDefaultAsync(p => p.CourtId == courtId && p.SlotId == slot.SlotId);
      decimal subTotal = pricing != null && pricing.Price > 0 ? pricing.Price : 0;
      var court = await _context.Courts.FindAsync(courtId);
      if (subTotal == 0 && court != null)
      {
        decimal hours = (decimal)(slot.EndTime - slot.StartTime).TotalHours;
        subTotal = (court.PricePerHour > 0 ? court.PricePerHour : 100000m) * (hours > 0 ? hours : 1.5m);
      }
      if (services != null && services.Any() && court != null)
      {
        var serviceIds = services.Select(s => s.ServiceId).ToList();
        var complexServices = await _context.ComplexCourtTypeServices
            .Include(cs => cs.Service)
            .Where(cs => cs.ComplexId == court.ComplexId && cs.CourtTypeId == court.CourtTypeId && serviceIds.Contains(cs.ServiceId))
            .ToListAsync();
            
        foreach (var item in services.Where(x => x.Quantity > 0))
        {
          var s = complexServices.FirstOrDefault(x => x.ServiceId == item.ServiceId);
          var serviceObj = s?.Service ?? await _context.Services.FindAsync(item.ServiceId);
          decimal price = s != null && s.Price > 0 ? s.Price : (serviceObj?.Price ?? 0);
          subTotal += price * item.Quantity;
        }
      }
      return subTotal;
    }

    /// <summary>Processes promotion application.</summary>
    private async Task<(int? promoId, decimal discount)> ProcessPromotionAsync(string? promoCode, decimal subTotal)
    {
      if (string.IsNullOrWhiteSpace(promoCode)) return (null, 0);
      var promo = await _promoRepo.GetByCodeAsync(promoCode);
      if (promo == null || !promo.IsActive || DateTime.UtcNow < promo.StartDate || DateTime.UtcNow > promo.EndDate)
        throw new ArgumentException("Mã giảm giá không hợp lệ hoặc đã hết hạn.");
      if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
        throw new ArgumentException("Mã giảm giá đã hết lượt sử dụng.");
      if (subTotal < promo.MinOrderAmount)
        throw new ArgumentException($"Đơn tối thiểu phải từ {promo.MinOrderAmount:N0}đ.");

      decimal discount = promo.DiscountType == DiscountType.Percent ? subTotal * (promo.DiscountValue / 100m) : promo.DiscountValue;
      if (promo.DiscountType == DiscountType.Percent && promo.MaxDiscount.HasValue && discount > promo.MaxDiscount.Value)
        discount = promo.MaxDiscount.Value;

      promo.UsedCount += 1;
      await _promoRepo.UpdateAsync(promo);
      return (promo.PromotionId, Math.Min(discount, subTotal));
    }

    /// <summary>Adds booking services entities.</summary>
    private async Task AddBookingServicesAsync(Booking booking, List<ServiceItemRequest>? services)
    {
      if (services == null || !services.Any()) return;
      var court = await _context.Courts.FindAsync(booking.CourtId);
      if (court == null) return;
      var serviceIds = services.Select(s => s.ServiceId).ToList();
      var complexServices = await _context.ComplexCourtTypeServices
            .Include(cs => cs.Service)
            .Where(cs => cs.ComplexId == court.ComplexId && cs.CourtTypeId == court.CourtTypeId && serviceIds.Contains(cs.ServiceId))
            .ToListAsync();
      foreach (var item in services.Where(x => x.Quantity > 0))
      {
        var s = complexServices.FirstOrDefault(x => x.ServiceId == item.ServiceId);
        var serviceObj = s?.Service ?? await _context.Services.FindAsync(item.ServiceId);
        if (serviceObj != null)
        {
          if (serviceObj.StockQty < item.Quantity)
          {
            throw new InvalidOperationException($"Số lượng hàng tồn kho không đủ cho dịch vụ '{serviceObj.ServiceName}'. Còn lại: {serviceObj.StockQty}, Yêu cầu: {item.Quantity}");
          }
          serviceObj.StockQty -= item.Quantity;
          _context.Services.Update(serviceObj);
          decimal price = s != null && s.Price > 0 ? s.Price : serviceObj.Price;
          booking.BookingServices.Add(new BookingService { ServiceId = item.ServiceId, Quantity = item.Quantity, TotalPrice = price * item.Quantity });
        }
      }
    }

    /// <summary>Handles cancellation refund logic.</summary>
    private async Task HandleCancellationAsync(Booking booking)
    {
      if (booking.Payment != null) booking.Payment.RefundAmount = 0;
      if (booking.PromotionId.HasValue)
      {
        var promo = await _promoRepo.GetByIdAsync(booking.PromotionId.Value);
        if (promo != null && promo.UsedCount > 0)
        {
          promo.UsedCount -= 1;
          await _promoRepo.UpdateAsync(promo);
        }
      }
    }

    private static string CleanSlotName(string? name)
    {
      if (string.IsNullOrWhiteSpace(name)) return string.Empty;
      int idx = name.IndexOf('(');
      return idx > 0 ? name.Substring(0, idx).Trim() : name.Trim();
    }

    private static string FormatSlotNameForBooking(Booking b)
    {
      string startName = CleanSlotName(b.TimeSlot?.SlotName);
      if (string.IsNullOrEmpty(startName)) startName = $"Slot {b.SlotId}";

      var durationHours = (b.EndTime - b.StartTime).TotalHours;
      if (durationHours > 1.75)
      {
        var digits = new string(startName.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out int startNum) && startNum > 0)
        {
          int slotCount = (int)Math.Round(durationHours / 1.5);
          if (slotCount > 1)
          {
            int endNum = startNum + slotCount - 1;
            return $"Slot {startNum} - Slot {endNum}";
          }
        }
      }
      return startName;
    }

    /// <summary>Maps booking entity to DTO.</summary>
    private static BookingDto MapToDto(Booking b)
    {
      return new BookingDto
      {
        BookingId = b.BookingId, BookingCode = b.BookingCode, UserId = b.UserId,
        CustomerName = b.User?.FullName ?? $"User #{b.UserId}", CustomerPhone = b.User?.Phone,
        CourtId = b.CourtId, CourtName = b.Court?.CourtName ?? $"Court #{b.CourtId}",
        SlotId = b.SlotId, SlotName = FormatSlotNameForBooking(b),
        BookingDate = b.BookingDate, StartTime = b.StartTime.ToString("hh\\:mm"), EndTime = b.EndTime.ToString("hh\\:mm"),
        SubTotal = b.SubTotal, DiscountAmount = b.DiscountAmount, TotalAmount = b.TotalAmount,
        Status = b.Status, PromotionId = b.PromotionId, PromotionCode = b.Promotion?.PromoCode,
        Note = b.Note, CancelReason = b.CancelReason, CreatedAt = b.CreatedAt,
        Payment = b.Payment == null ? null : new PaymentDto { PaymentId = b.Payment.PaymentId, Amount = b.Payment.Amount, PaymentMethod = b.Payment.PaymentMethod, TransactionId = b.Payment.TransactionId, Status = b.Payment.Status, RefundAmount = b.Payment.RefundAmount, PaidAt = b.Payment.PaidAt },
        Services = b.BookingServices?.Select(bs => new BookingServiceItemDto
        {
          ServiceId = bs.ServiceId,
          ServiceName = bs.Service?.ServiceName ?? $"Service #{bs.ServiceId}",
          Quantity = bs.Quantity,
          UnitPrice = bs.Quantity > 0 ? (bs.TotalPrice / bs.Quantity) : (bs.Service?.Price ?? 0),
          TotalPrice = bs.TotalPrice
        }).ToList() ?? new List<BookingServiceItemDto>()
      };
    }

    /// <summary>Gets list of tournaments belonging to a specific customer.</summary>
    public async Task<IEnumerable<TournamentDto>> GetCustomerTournamentsAsync(int userId)
    {
      var tournaments = await GetBaseTournamentQuery().Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToListAsync();
      return tournaments.Select(MapToTournamentDto).ToList();
    }

    /// <summary>Gets all tournaments with optional filters for admin and staff.</summary>
    public async Task<IEnumerable<TournamentDto>> GetAdminTournamentsAsync(DateTime? date, string? status)
    {
      var query = GetBaseTournamentQuery();
      if (date.HasValue) query = query.Where(t => t.Bookings.Any(b => b.BookingDate.Date == date.Value.Date));
      if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TournamentStatus>(status, true, out var statusEnum))
        query = query.Where(t => t.Status == statusEnum);

      var tournaments = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
      return tournaments.Select(MapToTournamentDto).ToList();
    }

    /// <summary>Gets base tournament query with includes.</summary>
    private IQueryable<Tournament> GetBaseTournamentQuery()
    {
      return _context.Tournaments.Include(t => t.User).Include(t => t.Bookings).ThenInclude(b => b.Court).Include(t => t.Bookings).ThenInclude(b => b.TimeSlot).Include(t => t.Bookings).ThenInclude(b => b.BookingServices).ThenInclude(bs => bs.Service);
    }

    /// <summary>Gets full tournament detail. Customer can only view their own; Admin/Staff can view all.</summary>
    public async Task<TournamentDto?> GetTournamentDetailAsync(int tournamentId, int userId, bool isAdminOrStaff)
    {
      var tournament = await _context.Tournaments.Include(t => t.User).Include(t => t.Bookings).ThenInclude(b => b.Court).Include(t => t.Bookings).ThenInclude(b => b.TimeSlot).Include(t => t.Bookings).ThenInclude(b => b.BookingServices).ThenInclude(bs => bs.Service).Include(t => t.Bookings).ThenInclude(b => b.Payment).FirstOrDefaultAsync(t => t.TournamentId == tournamentId);
      if (tournament == null) return null;
      if (!isAdminOrStaff && tournament.UserId != userId) throw new UnauthorizedAccessException("Bạn không có quyền xem giải đấu này.");
      return MapToTournamentDto(tournament);
    }

    /// <summary>Updates tournament status (Admin/Staff only). Cascades to child bookings when Cancelled or Paid.</summary>
    public async Task<TournamentDto?> UpdateTournamentStatusAsync(int tournamentId, UpdateTournamentStatusRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
      var tournament = await GetBaseTournamentQuery().FirstOrDefaultAsync(t => t.TournamentId == tournamentId);
      if (tournament == null) return null;

      var strategy = _context.Database.CreateExecutionStrategy();
      var result = await strategy.ExecuteAsync(async () =>
      {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
          tournament.Status = request.Status;
          ApplyStatusCascadeToBookings(tournament, request);
          await _context.SaveChangesAsync();
          await transaction.CommitAsync();
          _cache.Remove(PublicTournamentsCacheKey);
          return MapToTournamentDto(tournament);
        }
        catch
        {
          await transaction.RollbackAsync();
          throw;
        }
      });

      if (request.Status is TournamentStatus.Cancelled or TournamentStatus.Paid or TournamentStatus.Confirmed)
      {
        var slotStatus = request.Status == TournamentStatus.Cancelled ? "Available" : "Booked";
        await BroadcastTournamentSlotStatusesAsync(result.Bookings, slotStatus);
      }

      return result;
    }

    /// <summary>Applies cascade status updates to child bookings.</summary>
    private static void ApplyStatusCascadeToBookings(Tournament tournament, UpdateTournamentStatusRequest request)
    {
      if (request.Status == TournamentStatus.Cancelled)
      {
        foreach (var booking in tournament.Bookings.Where(b => b.Status != BookingStatus.Cancelled))
        {
          booking.Status = BookingStatus.Cancelled;
          booking.CancelReason = request.CancelReason ?? "Giải đấu bị hủy.";

        }
      }
      else if (request.Status == TournamentStatus.Paid || request.Status == TournamentStatus.Confirmed)
      {
        foreach (var booking in tournament.Bookings.Where(b => b.Status == BookingStatus.Pending))
        {
          booking.Status = BookingStatus.Confirmed;
        }
      }
    }

    /// <summary>Maps tournament entity to TournamentDto.</summary>
    private static TournamentDto MapToTournamentDto(Tournament t)
    {
      return new TournamentDto
      {
        TournamentId = t.TournamentId, TournamentName = t.TournamentName, Description = t.Description,
        UserId = t.UserId, CustomerName = t.User?.FullName ?? $"User #{t.UserId}", TotalAmount = t.TotalAmount,
        Status = t.Status, CreatedAt = t.CreatedAt, ExpiredAt = t.ExpiredAt, Bookings = t.Bookings?.Select(MapToDto).ToList() ?? new List<BookingDto>()
      };
    }

    /// <summary>Maps tournament entity to TournamentDto with explicit customer name.</summary>
    private static TournamentDto MapToTournamentDtoWithCustomer(Tournament t, string customerName, List<Booking> bookings)
    {
      return new TournamentDto
      {
        TournamentId = t.TournamentId, TournamentName = t.TournamentName, Description = t.Description,
        UserId = t.UserId, CustomerName = customerName, TotalAmount = t.TotalAmount,
        Status = t.Status, CreatedAt = t.CreatedAt, ExpiredAt = t.ExpiredAt, Bookings = bookings.Select(MapToDto).ToList()
      };
    }

    /// <summary>Gets public tournament info visible to any authenticated customer.</summary>
    public async Task<TournamentPublicDto?> GetTournamentPublicInfoAsync(int tournamentId)
    {
      var tournament = await GetBaseTournamentQuery().FirstOrDefaultAsync(t => t.TournamentId == tournamentId);
      return tournament == null ? null : MapToPublicDto(tournament);
    }

    /// <summary>Gets paged customer bookings with database filtering.</summary>
    public async Task<PagedResult<BookingDto>> GetPagedCustomerBookingsAsync(int userId, BookingFilterParams filter)
    {
      var now = DateTime.UtcNow;
      var expiredBookings = await _context.Bookings
          .Where(b => b.UserId == userId && b.Status == BookingStatus.Pending && b.ExpiredAt.HasValue && b.ExpiredAt.Value <= now)
          .ToListAsync();

      if (expiredBookings.Any())
      {
        foreach (var exp in expiredBookings)
        {
          exp.Status = BookingStatus.Cancelled;
          exp.CancelReason = "Hết hạn thanh toán (Quá 5 phút)";
        }
        await _context.SaveChangesAsync();
      }

      var query = _context.Bookings.Include(b => b.User).Include(b => b.Court).Include(b => b.TimeSlot).Include(b => b.Promotion).Include(b => b.Payment).Include(b => b.BookingServices).ThenInclude(bs => bs.Service).Where(b => b.UserId == userId).AsQueryable();
      if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<BookingStatus>(filter.Status, true, out var st))
      {
          query = query.Where(b => b.Status == st);
      }
      return await FilterAndPageBookingsQueryAsync(query, filter);
    }

    /// <summary>Gets paged admin bookings with database filtering.</summary>
    public async Task<PagedResult<BookingDto>> GetPagedAdminBookingsAsync(BookingFilterParams filter)
    {
      var query = _context.Bookings.Include(b => b.User).Include(b => b.Court).Include(b => b.TimeSlot).Include(b => b.Promotion).Include(b => b.Payment).Include(b => b.BookingServices).ThenInclude(bs => bs.Service).AsQueryable();
      if (filter.CourtTypeId.HasValue) query = query.Where(b => b.Court != null && b.Court.CourtTypeId == filter.CourtTypeId.Value);
      if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<BookingStatus>(filter.Status, true, out var st)) query = query.Where(b => b.Status == st);
      return await FilterAndPageBookingsQueryAsync(query, filter);
    }

    /// <summary>Filters and pages bookings database query.</summary>
    private static async Task<PagedResult<BookingDto>> FilterAndPageBookingsQueryAsync(IQueryable<Booking> query, BookingFilterParams filter)
    {
      if (filter.FromDate.HasValue) query = query.Where(b => b.BookingDate.Date >= filter.FromDate.Value.Date);
      if (filter.ToDate.HasValue) query = query.Where(b => b.BookingDate.Date <= filter.ToDate.Value.Date);
      if (!string.IsNullOrWhiteSpace(filter.Keyword))
      {
        var kw = filter.Keyword.Trim().ToLower();
        query = query.Where(b => b.BookingCode.ToLower().Contains(kw) || (b.User != null && b.User.FullName.ToLower().Contains(kw)) || (b.User != null && b.User.Phone != null && b.User.Phone.Contains(kw)) || (b.Court != null && b.Court.CourtName.ToLower().Contains(kw)));
      }
      var total = await query.CountAsync();
      var items = await query.OrderByDescending(b => b.CreatedAt).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
      return new PagedResult<BookingDto> { Items = items.Select(MapToDto).ToList(), TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    /// <summary>Gets paged customer tournaments with database filtering.</summary>
    public async Task<PagedResult<TournamentDto>> GetPagedCustomerTournamentsAsync(int userId, TournamentFilterParams filter)
    {
      var query = GetBaseTournamentQuery().Where(t => t.UserId == userId);
      if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<TournamentStatus>(filter.Status, true, out var st))
        query = query.Where(t => t.Status == st);
      return await FilterAndPageTournamentsQueryAsync(query, filter);
    }

    /// <summary>Gets paged admin tournaments with database filtering.</summary>
    public async Task<PagedResult<TournamentDto>> GetPagedAdminTournamentsAsync(TournamentFilterParams filter)
    {
      var query = GetBaseTournamentQuery();
      if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<TournamentStatus>(filter.Status, true, out var st))
        query = query.Where(t => t.Status == st);
      return await FilterAndPageTournamentsQueryAsync(query, filter);
    }

    /// <summary>Filters and pages tournaments database query.</summary>
    private static async Task<PagedResult<TournamentDto>> FilterAndPageTournamentsQueryAsync(IQueryable<Tournament> query, TournamentFilterParams filter)
    {
      if (filter.FromDate.HasValue) query = query.Where(t => t.CreatedAt.Date >= filter.FromDate.Value.Date);
      if (filter.ToDate.HasValue) query = query.Where(t => t.CreatedAt.Date <= filter.ToDate.Value.Date);
      if (!string.IsNullOrWhiteSpace(filter.Keyword))
      {
        var kw = filter.Keyword.Trim().ToLower();
        query = query.Where(t => t.TournamentName.ToLower().Contains(kw) || (t.User != null && t.User.FullName.ToLower().Contains(kw)));
      }
      var total = await query.CountAsync();
      var items = await query.OrderByDescending(t => t.CreatedAt).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
      return new PagedResult<TournamentDto> { Items = items.Select(MapToTournamentDto).ToList(), TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    /// <summary>Gets paged public tournaments with in-memory caching.</summary>
    public async Task<PagedResult<TournamentPublicDto>> GetPagedPublicTournamentsAsync(TournamentFilterParams filter)
    {
      if (!_cache.TryGetValue(PublicTournamentsCacheKey, out List<TournamentPublicDto>? allPublic) || allPublic == null)
      {
        var dbTournaments = await GetBaseTournamentQuery()
          .Where(t => t.Status == TournamentStatus.Paid || t.Status == TournamentStatus.Confirmed)
          .ToListAsync();
        var today = DateTime.UtcNow.Date;
        allPublic = dbTournaments
          .Select(MapToPublicDto)
          .Select(t =>
          {
            t.Courts = t.Courts
              .Where(c => c.Status != BookingStatus.Cancelled)
              .OrderBy(c => c.BookingDate)
              .ThenBy(c => c.StartTime)
              .ToList();
            return t;
          })
          .OrderBy(t => t.Courts
            .Where(c => c.Status != BookingStatus.Cancelled && c.BookingDate.Date >= today)
            .Select(c => c.BookingDate.Date)
            .DefaultIfEmpty(DateTime.MaxValue)
            .Min())
          .ThenByDescending(t => t.Courts
            .Where(c => c.Status != BookingStatus.Cancelled)
            .Select(c => c.BookingDate.Date)
            .DefaultIfEmpty(DateTime.MinValue)
            .Max())
          .ThenByDescending(t => t.CreatedAt)
          .ToList();
        _cache.Set(PublicTournamentsCacheKey, allPublic, TimeSpan.FromMinutes(10));
      }
      var query = allPublic.AsEnumerable();
      if (!string.IsNullOrWhiteSpace(filter.Keyword))
      {
        var kw = filter.Keyword.Trim().ToLower();
        query = query.Where(t => t.TournamentName.ToLower().Contains(kw) || (t.OrganizerName != null && t.OrganizerName.ToLower().Contains(kw)));
      }
      var total = query.Count();
      var items = query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList();
      return new PagedResult<TournamentPublicDto> { Items = items, TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    /// <summary>Helper to map tournament entity to public DTO.</summary>
    private static TournamentPublicDto MapToPublicDto(Tournament t)
    {
      return new TournamentPublicDto
      {
        TournamentId = t.TournamentId, TournamentName = t.TournamentName, Description = t.Description,
        OrganizerName = t.User?.FullName ?? "Ẩn danh", Status = t.Status, CreatedAt = t.CreatedAt,
        Courts = t.Bookings?.Select(b => new CourtSlotPublicDto { CourtId = b.CourtId, CourtName = b.Court?.CourtName ?? $"Sân #{b.CourtId}", SlotId = b.SlotId, SlotName = b.TimeSlot?.SlotName ?? $"{b.StartTime:hh\\:mm}-{b.EndTime:hh\\:mm}", StartTime = b.StartTime.ToString("hh\\:mm"), EndTime = b.EndTime.ToString("hh\\:mm"), BookingDate = b.BookingDate, Status = b.Status }).ToList() ?? new List<CourtSlotPublicDto>()
      };
    }

    /// <summary>Adds services to an existing booking.</summary>
    public async Task<BookingDto?> AddServicesToBookingAsync(int bookingId, Dictionary<int, int> serviceQuantities)
    {
      var booking = await _bookingRepo.GetDetailAsync(bookingId);
      if (booking == null) return null;

      var court = await _context.Courts.FindAsync(booking.CourtId);

      decimal additionalAmount = 0;
      foreach (var kvp in serviceQuantities.Where(q => q.Value > 0))
      {
        var service = await _context.Services.FindAsync(kvp.Key);
        if (service == null) throw new ArgumentException($"Dịch vụ #{kvp.Key} không tồn tại.");

        ComplexCourtTypeService? offering = null;
        if (court != null)
        {
          offering = await _context.ComplexCourtTypeServices
              .FirstOrDefaultAsync(cs => cs.ComplexId == court.ComplexId && cs.CourtTypeId == court.CourtTypeId && cs.ServiceId == kvp.Key && cs.IsActive);
          if (offering == null)
          {
            offering = await _context.ComplexCourtTypeServices
                .FirstOrDefaultAsync(cs => cs.ComplexId == court.ComplexId && cs.ServiceId == kvp.Key && cs.IsActive);
          }
        }

        int availableStock = (offering != null && offering.StockQty > 0) ? offering.StockQty : service.StockQty;

        if (availableStock < kvp.Value)
          throw new InvalidOperationException($"Số lượng hàng tồn kho không đủ cho dịch vụ '{service.ServiceName}'. Còn lại: {availableStock}, Yêu cầu: {kvp.Value}");

        // Deduct stock
        if (offering != null && offering.StockQty > 0)
        {
          offering.StockQty -= kvp.Value;
          _context.ComplexCourtTypeServices.Update(offering);
        }
        service.StockQty = Math.Max(0, service.StockQty - kvp.Value);
        _context.Services.Update(service);

        decimal unitPrice = (offering != null && offering.Price > 0) ? offering.Price : service.Price;

        // Check if service already exists in booking
        var existingService = booking.BookingServices.FirstOrDefault(bs => bs.ServiceId == kvp.Key);
        if (existingService != null)
        {
          existingService.Quantity += kvp.Value;
          existingService.TotalPrice += unitPrice * kvp.Value;
        }
        else
        {
          var bookingService = new BookingService
          {
            BookingId = bookingId,
            ServiceId = kvp.Key,
            Quantity = kvp.Value,
            TotalPrice = unitPrice * kvp.Value
          };
          booking.BookingServices.Add(bookingService);
        }

        additionalAmount += unitPrice * kvp.Value;
      }

      booking.TotalAmount += additionalAmount;
      booking.SubTotal += additionalAmount;

      var addedServicesText = string.Join(", ", serviceQuantities.Where(q => q.Value > 0).Select(q => {
          var s = _context.Services.Find(q.Key);
          return $"{s?.ServiceName ?? "Dịch vụ"} x{q.Value}";
      }));
      booking.Note = string.IsNullOrEmpty(booking.Note)
          ? $"Đặt thêm: {addedServicesText}"
          : $"{booking.Note} | Đặt thêm: {addedServicesText}";

      await _context.SaveChangesAsync();
      var updated = await _bookingRepo.GetDetailAsync(bookingId);
      return MapToDto(updated ?? booking);
    }

    /// <summary>Joins FIFO Waitlist queue for a booked slot.</summary>
    public async Task<WaitlistResponseDto> JoinWaitlistAsync(int userId, CreateWaitlistRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
      var court = await _context.Courts.FindAsync(request.CourtId) ?? throw new ArgumentException("Sân không tồn tại.");
      var slot = await _context.TimeSlots.FindAsync(request.SlotId) ?? throw new ArgumentException("Khung giờ không hợp lệ.");

      // Check if user is already in waitlist for this slot/date
      var existing = await _context.Waitlists.FirstOrDefaultAsync(w =>
        w.UserId == userId && w.CourtId == request.CourtId && w.SlotId == request.SlotId && w.WaitDate.Date == request.WaitDate.Date && w.Status == WaitlistStatus.Waiting);

      if (existing != null)
      {
        return new WaitlistResponseDto
        {
          WaitlistId = existing.WaitlistId,
          UserId = existing.UserId,
          CourtId = existing.CourtId,
          CourtName = court.CourtName,
          SlotId = existing.SlotId,
          SlotName = slot.SlotName,
          WaitDate = existing.WaitDate,
          Position = existing.Position,
          Status = existing.Status.ToString()
        };
      }

      // Compute next FIFO position for this court/slot/date
      var maxPos = await _context.Waitlists
        .Where(w => w.CourtId == request.CourtId && w.SlotId == request.SlotId && w.WaitDate.Date == request.WaitDate.Date)
        .MaxAsync(w => (int?)w.Position) ?? 0;

      int nextPosition = maxPos + 1;

      var waitlist = new Waitlist
      {
        UserId = userId,
        CourtId = request.CourtId,
        SlotId = request.SlotId,
        WaitDate = request.WaitDate.Date,
        Position = nextPosition,
        Status = WaitlistStatus.Waiting
      };

      await _context.Waitlists.AddAsync(waitlist);
      await _context.SaveChangesAsync();

      return new WaitlistResponseDto
      {
        WaitlistId = waitlist.WaitlistId,
        UserId = waitlist.UserId,
        CourtId = waitlist.CourtId,
        CourtName = court.CourtName,
        SlotId = waitlist.SlotId,
        SlotName = slot.SlotName,
        WaitDate = waitlist.WaitDate,
        Position = waitlist.Position,
        Status = waitlist.Status.ToString()
      };
    }
  }
}
