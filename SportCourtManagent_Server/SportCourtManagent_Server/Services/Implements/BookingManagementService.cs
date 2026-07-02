using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
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
    private readonly IHubContext<SlotStatusHub> _hubContext;

    public BookingManagementService(IBookingRepository bookingRepo, IPromotionRepository promoRepo, AppDbContext context, IHubContext<SlotStatusHub> hubContext)
    {
      _bookingRepo = bookingRepo ?? throw new ArgumentNullException(nameof(bookingRepo));
      _promoRepo = promoRepo ?? throw new ArgumentNullException(nameof(promoRepo));
      _context = context ?? throw new ArgumentNullException(nameof(context));
      _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
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

      var slot = await _context.TimeSlots.FindAsync(request.SlotId);
      if (slot == null) throw new ArgumentException("Khung giờ không hợp lệ.");

      // Check for duplicate slot booking
      var isBooked = await _context.Bookings.AnyAsync(b =>
        b.CourtId == request.CourtId
        && b.SlotId == request.SlotId
        && b.BookingDate.Date == request.BookingDate.Date
        && b.Status != BookingStatus.Cancelled);
      if (isBooked)
        throw new ArgumentException($"Khung giờ {slot.SlotName} ngày {request.BookingDate:dd/MM/yyyy} đã có người đặt.");

      decimal subTotal = await CalculateSubTotalAsync(request.CourtId, slot, request.ServiceIds);
      var (promoId, discountAmount) = await ProcessPromotionAsync(request.PromotionCode, subTotal);

      var booking = new Booking
      {
        BookingCode = $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}",
        UserId = userId,
        CourtId = request.CourtId,
        SlotId = request.SlotId,
        BookingDate = request.BookingDate,
        StartTime = slot.StartTime,
        EndTime = slot.EndTime,
        SubTotal = subTotal,
        DiscountAmount = discountAmount,
        TotalAmount = Math.Max(0, subTotal - discountAmount),
        Status = BookingStatus.Pending,
        PromotionId = promoId,
        Note = request.Note,
        CreatedAt = DateTime.UtcNow
      };

      await AddBookingServicesAsync(booking, request.ServiceIds);
      await _bookingRepo.AddAsync(booking);

      // Push SignalR slot status update
      await _hubContext.Clients.Group($"court-{request.CourtId}")
        .SendAsync("SlotStatusChanged", request.CourtId, request.SlotId, request.BookingDate.ToString("yyyy-MM-dd"), "Booked");

      return MapToDto(await _bookingRepo.GetDetailAsync(booking.BookingId) ?? booking);
    }

    /// <summary>Creates recurring bookings across multiple weeks. Skips conflicting dates (Option A).</summary>
    public async Task<RecurringBookingResponseDto> CreateRecurringBookingAsync(int userId, CreateRecurringBookingRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
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

      // Generate all target dates from StartDate to EndDate matching DaysOfWeek
      var allDates = new List<DateTime>();
      for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
      {
        if (request.DaysOfWeek.Contains((int)date.DayOfWeek))
          allDates.Add(date);
      }

      if (!allDates.Any())
        throw new ArgumentException("Không có ngày nào phù hợp trong khoảng thời gian đã chọn.");

      // Batch check all conflicting dates
      var existingBookings = await _context.Bookings
        .Where(b => b.CourtId == request.CourtId
                  && b.SlotId == request.SlotId
                  && allDates.Contains(b.BookingDate)
                  && b.Status != BookingStatus.Cancelled)
        .Select(b => b.BookingDate.Date)
        .ToListAsync();

      var conflictDates = allDates.Where(d => existingBookings.Contains(d.Date)).ToList();
      var availableDates = allDates.Where(d => !existingBookings.Contains(d.Date)).ToList();

      if (!availableDates.Any())
        throw new ArgumentException("Tất cả các ngày trong khoảng thời gian đã chọn đều đã có người đặt.");

      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        // Create RecurringBooking parent record
        var daysStr = string.Join(",", request.DaysOfWeek.OrderBy(d => d));
        var recurring = new RecurringBooking
        {
          UserId = userId,
          CourtId = request.CourtId,
          SlotId = request.SlotId,
          StartDate = request.StartDate,
          EndDate = request.EndDate,
          DaysOfWeek = daysStr,
          Status = RecurringBookingStatus.Active
        };
        await _context.RecurringBookings.AddAsync(recurring);
        await _context.SaveChangesAsync();

        // Create individual bookings for each available date
        decimal totalAmount = 0;
        var createdBookings = new List<Booking>();

        foreach (var date in availableDates)
        {
          decimal subTotal = await CalculateSubTotalAsync(request.CourtId, slot, null);

          var booking = new Booking
          {
            BookingCode = $"RBK{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}",
            UserId = userId,
            CourtId = request.CourtId,
            SlotId = request.SlotId,
            BookingDate = date,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            SubTotal = subTotal,
            DiscountAmount = 0,
            TotalAmount = subTotal,
            Status = BookingStatus.Pending,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
          };

          totalAmount += subTotal;
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

        return new RecurringBookingResponseDto
        {
          RecurringId = recurring.RecurringId,
          CourtId = court.CourtId,
          CourtName = court.CourtName,
          SlotId = slot.SlotId,
          SlotName = slot.SlotName,
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
    }

    /// <summary>Updates booking status asynchronous.</summary>
    public async Task<BookingDto?> UpdateBookingStatusAsync(int id, UpdateBookingStatusRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));

      var booking = await _bookingRepo.GetDetailAsync(id);
      if (booking == null) return null;

      if (request.Status == BookingStatus.Cancelled && booking.Status != BookingStatus.Cancelled)
      {
        booking.Status = BookingStatus.Cancelled;
        booking.CancelReason = request.CancelReason;
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

    /// <summary>Creates a new tournament booking asynchronous.</summary>
    public async Task<TournamentDto> CreateTournamentAsync(int userId, CreateTournamentRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
      if (request.CourtSelections == null || !request.CourtSelections.Any())
        throw new ArgumentException("Giải đấu phải có ít nhất một sân được chọn.");

      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        decimal totalTournamentAmount = 0;
        var tournament = new Tournament
        {
          TournamentName = request.TournamentName,
          Description = request.Description,
          UserId = userId,
          Status = TournamentStatus.Pending,
          CreatedAt = DateTime.UtcNow
        };
        await _context.Tournaments.AddAsync(tournament);
        await _context.SaveChangesAsync(); // Get TournamentId

        var createdBookings = new List<Booking>();
        var user = await _context.Users.FindAsync(userId);
        
        foreach (var courtSelection in request.CourtSelections)
        {
          if (courtSelection.SlotIds == null || !courtSelection.SlotIds.Any()) continue;

          foreach (var slotId in courtSelection.SlotIds)
          {
            var slot = await _context.TimeSlots.FindAsync(slotId);
            if (slot == null) throw new ArgumentException($"Khung giờ {slotId} không hợp lệ.");

            var isBooked = await _context.Bookings.AnyAsync(b => b.CourtId == courtSelection.CourtId 
                                                              && b.SlotId == slotId 
                                                              && b.BookingDate.Date == request.BookingDate.Date 
                                                              && b.Status != BookingStatus.Cancelled);
            if (isBooked)
            {
                throw new ArgumentException($"Sân {courtSelection.CourtId} vào khung giờ {slot.SlotName} ngày {request.BookingDate:dd/MM/yyyy} đã có người đặt.");
            }

            decimal subTotal = await CalculateSubTotalAsync(courtSelection.CourtId, slot, request.Services);
            totalTournamentAmount += subTotal;

            var booking = new Booking
            {
              BookingCode = $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}",
              UserId = userId,
              CourtId = courtSelection.CourtId,
              SlotId = slotId,
              BookingDate = request.BookingDate,
              StartTime = slot.StartTime,
              EndTime = slot.EndTime,
              SubTotal = subTotal,
              DiscountAmount = 0,
              TotalAmount = subTotal,
              Status = BookingStatus.Pending,
              TournamentId = tournament.TournamentId,
              Note = request.Note,
              CreatedAt = DateTime.UtcNow
            };

            await AddBookingServicesAsync(booking, request.Services);
            await _context.Bookings.AddAsync(booking);
            createdBookings.Add(booking);
          }
        }
        await _context.SaveChangesAsync();

        decimal discountAmount = 0;
        int? promoId = null;
        if (!string.IsNullOrWhiteSpace(request.PromotionCode))
        {
            var promoProcess = await ProcessPromotionAsync(request.PromotionCode, totalTournamentAmount);
            promoId = promoProcess.promoId;
            discountAmount = promoProcess.discount;

            if (promoId.HasValue && createdBookings.Any())
            {
                decimal perBookingDiscount = discountAmount / createdBookings.Count;
                foreach (var b in createdBookings)
                {
                    b.PromotionId = promoId;
                    b.DiscountAmount = perBookingDiscount;
                    b.TotalAmount = Math.Max(0, b.SubTotal - b.DiscountAmount);
                }
                await _context.SaveChangesAsync();
            }
        }

        tournament.TotalAmount = Math.Max(0, totalTournamentAmount - discountAmount);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new TournamentDto
        {
          TournamentId = tournament.TournamentId,
          TournamentName = tournament.TournamentName,
          Description = tournament.Description,
          UserId = tournament.UserId,
          CustomerName = user?.FullName ?? $"User #{tournament.UserId}",
          TotalAmount = tournament.TotalAmount,
          Status = tournament.Status,
          CreatedAt = tournament.CreatedAt,
          Bookings = createdBookings.Select(MapToDto).ToList()
        };
      }
      catch (Exception)
      {
        await transaction.RollbackAsync();
        throw;
      }
    }

    /// <summary>Calculates subtotal from court pricing and services.</summary>
    private async Task<decimal> CalculateSubTotalAsync(int courtId, TimeSlot slot, List<ServiceItemRequest>? services)
    {
      var pricing = await _context.CourtPricings
        .FirstOrDefaultAsync(p => p.CourtId == courtId && p.SlotId == slot.SlotId);

      decimal subTotal = 0;
      if (pricing != null)
      {
        subTotal = pricing.Price;
      }
      else
      {
        var court = await _context.Courts.FindAsync(courtId);
        decimal hours = (decimal)(slot.EndTime - slot.StartTime).TotalHours;
        subTotal = (court?.PricePerHour ?? 0) * (hours > 0 ? hours : 1);
      }

      if (services != null && services.Any())
      {
        var serviceIds = services.Select(s => s.ServiceId).ToList();
        var dbServices = await _context.Services.Where(s => serviceIds.Contains(s.ServiceId)).ToListAsync();
        foreach (var item in services)
        {
          var s = dbServices.FirstOrDefault(x => x.ServiceId == item.ServiceId);
          if (s != null) subTotal += s.Price * item.Quantity;
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
      {
        throw new ArgumentException("Mã giảm giá không hợp lệ hoặc đã hết hạn.");
      }
      if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
      {
        throw new ArgumentException("Mã giảm giá đã hết lượt sử dụng.");
      }
      if (subTotal < promo.MinOrderAmount)
      {
        throw new ArgumentException($"Đơn tối thiểu phải từ {promo.MinOrderAmount:N0}đ.");
      }

      decimal discount = promo.DiscountType == DiscountType.Percent
        ? subTotal * (promo.DiscountValue / 100m)
        : promo.DiscountValue;

      if (promo.DiscountType == DiscountType.Percent && promo.MaxDiscount.HasValue && discount > promo.MaxDiscount.Value)
      {
        discount = promo.MaxDiscount.Value;
      }

      promo.UsedCount += 1;
      await _promoRepo.UpdateAsync(promo);
      return (promo.PromotionId, Math.Min(discount, subTotal));
    }

    /// <summary>Adds booking services entities.</summary>
    private async Task AddBookingServicesAsync(Booking booking, List<ServiceItemRequest>? services)
    {
      if (services == null || !services.Any()) return;
      var serviceIds = services.Select(s => s.ServiceId).ToList();
      var dbServices = await _context.Services.Where(s => serviceIds.Contains(s.ServiceId)).ToListAsync();
      foreach (var item in services)
      {
        var s = dbServices.FirstOrDefault(x => x.ServiceId == item.ServiceId);
        if (s != null && item.Quantity > 0)
        {
          booking.BookingServices.Add(new BookingService
          {
            ServiceId = item.ServiceId,
            Quantity = item.Quantity,
            TotalPrice = s.Price * item.Quantity
          });
        }
      }
    }

    /// <summary>Handles cancellation refund logic.</summary>
    private async Task HandleCancellationAsync(Booking booking)
    {
      if (booking.Payment != null)
      {
        booking.Payment.RefundAmount = 0;
      }
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

    /// <summary>Maps booking entity to DTO.</summary>
    private static BookingDto MapToDto(Booking b)
    {
      return new BookingDto
      {
        BookingId = b.BookingId,
        BookingCode = b.BookingCode,
        UserId = b.UserId,
        CustomerName = b.User?.FullName ?? $"User #{b.UserId}",
        CustomerPhone = b.User?.Phone,
        CourtId = b.CourtId,
        CourtName = b.Court?.CourtName ?? $"Court #{b.CourtId}",
        SlotId = b.SlotId,
        SlotName = b.TimeSlot?.SlotName ?? $"{b.StartTime:hh\\:mm} - {b.EndTime:hh\\:mm}",
        BookingDate = b.BookingDate,
        StartTime = b.StartTime.ToString("hh\\:mm"),
        EndTime = b.EndTime.ToString("hh\\:mm"),
        SubTotal = b.SubTotal,
        DiscountAmount = b.DiscountAmount,
        TotalAmount = b.TotalAmount,
        Status = b.Status,
        PromotionId = b.PromotionId,
        PromotionCode = b.Promotion?.PromoCode,
        Note = b.Note,
        CancelReason = b.CancelReason,
        CreatedAt = b.CreatedAt,
        Payment = b.Payment == null ? null : new PaymentDto
        {
          PaymentId = b.Payment.PaymentId,
          Amount = b.Payment.Amount,
          PaymentMethod = b.Payment.PaymentMethod,
          TransactionId = b.Payment.TransactionId,
          Status = b.Payment.Status,
          RefundAmount = b.Payment.RefundAmount,
          PaidAt = b.Payment.PaidAt
        }
      };
    }
    /// <summary>Gets list of tournaments belonging to a specific customer.</summary>
    public async Task<IEnumerable<TournamentDto>> GetCustomerTournamentsAsync(int userId)
    {
      var tournaments = await _context.Tournaments
        .Include(t => t.User)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.Court)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.TimeSlot)
        .Where(t => t.UserId == userId)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

      return tournaments.Select(MapToTournamentDto).ToList();
    }

    /// <summary>Gets all tournaments with optional filters for admin and staff.</summary>
    public async Task<IEnumerable<TournamentDto>> GetAdminTournamentsAsync(DateTime? date, string? status)
    {
      var query = _context.Tournaments
        .Include(t => t.User)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.Court)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.TimeSlot)
        .AsQueryable();

      if (date.HasValue)
        query = query.Where(t => t.Bookings.Any(b => b.BookingDate.Date == date.Value.Date));

      if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TournamentStatus>(status, true, out var statusEnum))
        query = query.Where(t => t.Status == statusEnum);

      var tournaments = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
      return tournaments.Select(MapToTournamentDto).ToList();
    }

    /// <summary>Gets full tournament detail. Customer can only view their own; Admin/Staff can view all.</summary>
    public async Task<TournamentDto?> GetTournamentDetailAsync(int tournamentId, int userId, bool isAdminOrStaff)
    {
      var tournament = await _context.Tournaments
        .Include(t => t.User)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.Court)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.TimeSlot)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.BookingServices)
            .ThenInclude(bs => bs.Service)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.Payment)
        .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);

      if (tournament == null) return null;

      // Customer chỉ xem được giải đấu của chính mình
      if (!isAdminOrStaff && tournament.UserId != userId)
        throw new UnauthorizedAccessException("Bạn không có quyền xem giải đấu này.");

      return MapToTournamentDto(tournament);
    }

    /// <summary>Updates tournament status (Admin/Staff only). Cascades to child bookings when Cancelled or Paid.</summary>
    public async Task<TournamentDto?> UpdateTournamentStatusAsync(int tournamentId, UpdateTournamentStatusRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));

      var tournament = await _context.Tournaments
        .Include(t => t.User)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.Court)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.TimeSlot)
        .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);

      if (tournament == null) return null;

      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        tournament.Status = request.Status;

        // Cascade: hủy giải đấu → hủy tất cả booking con
        if (request.Status == TournamentStatus.Cancelled)
        {
          foreach (var booking in tournament.Bookings.Where(b => b.Status != BookingStatus.Cancelled))
          {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelReason = request.CancelReason ?? "Giải đấu bị hủy.";
          }
        }
        // Cascade: thanh toán thành công → đánh dấu tất cả booking con là Confirmed
        else if (request.Status == TournamentStatus.Paid)
        {
          foreach (var booking in tournament.Bookings.Where(b => b.Status == BookingStatus.Pending))
          {
            booking.Status = BookingStatus.Confirmed;
          }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToTournamentDto(tournament);
      }
      catch (Exception)
      {
        await transaction.RollbackAsync();
        throw;
      }
    }

    /// <summary>Updates tournament info, courts, slots and services (Customer only, within 24h of creation).</summary>
    public async Task<TournamentDto?> UpdateTournamentInfoAsync(int tournamentId, int userId, UpdateTournamentInfoRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
      if (request.CourtSelections == null || !request.CourtSelections.Any())
        throw new ArgumentException("Giải đấu phải có ít nhất một sân được chọn.");

      var tournament = await _context.Tournaments
        .Include(t => t.User)
        .Include(t => t.Bookings)
          .ThenInclude(b => b.BookingServices)
        .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);

      if (tournament == null) return null;
      if (tournament.UserId != userId)
        throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa giải đấu này.");
      if (tournament.Status == TournamentStatus.Cancelled)
        throw new ArgumentException("Không thể chỉnh sửa giải đấu đã bị hủy.");
      if (DateTime.UtcNow - tournament.CreatedAt > TimeSpan.FromHours(24))
        throw new ArgumentException("Chỉ được phép chỉnh sửa thông tin trong vòng 24 giờ kể từ khi tạo giải đấu.");

      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        // Xóa tất cả booking con cũ (cascade xóa BookingServices theo)
        _context.Bookings.RemoveRange(tournament.Bookings);
        await _context.SaveChangesAsync();

        // Cập nhật thông tin cơ bản
        tournament.TournamentName = request.TournamentName;
        tournament.Description = request.Description;

        // Tạo lại các booking con theo lựa chọn mới
        decimal totalAmount = 0;
        var newBookings = new List<Booking>();

        foreach (var courtSelection in request.CourtSelections)
        {
          if (courtSelection.SlotIds == null || !courtSelection.SlotIds.Any()) continue;

          foreach (var slotId in courtSelection.SlotIds)
          {
            var slot = await _context.TimeSlots.FindAsync(slotId);
            if (slot == null) throw new ArgumentException($"Khung giờ {slotId} không hợp lệ.");

            var isBooked = await _context.Bookings.AnyAsync(b =>
              b.CourtId == courtSelection.CourtId
              && b.SlotId == slotId
              && b.BookingDate.Date == request.BookingDate.Date
              && b.Status != BookingStatus.Cancelled);
            if (isBooked)
              throw new ArgumentException($"Sân {courtSelection.CourtId} vào khung giờ {slot.SlotName} ngày {request.BookingDate:dd/MM/yyyy} đã có người đặt.");

            decimal subTotal = await CalculateSubTotalAsync(courtSelection.CourtId, slot, request.Services);
            totalAmount += subTotal;

            var booking = new Booking
            {
              BookingCode = $"TBK{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}",
              UserId = userId,
              CourtId = courtSelection.CourtId,
              SlotId = slotId,
              BookingDate = request.BookingDate,
              StartTime = slot.StartTime,
              EndTime = slot.EndTime,
              SubTotal = subTotal,
              DiscountAmount = 0,
              TotalAmount = subTotal,
              Status = BookingStatus.Pending,
              TournamentId = tournament.TournamentId,
              Note = request.Note,
              CreatedAt = DateTime.UtcNow
            };
            await AddBookingServicesAsync(booking, request.Services);
            newBookings.Add(booking);
          }
        }

        await _context.Bookings.AddRangeAsync(newBookings);

        // Áp dụng Promotion cho tổng đơn mới
        var (promoId, discountAmount) = await ProcessPromotionAsync(request.PromotionCode, totalAmount);
        tournament.TotalAmount = Math.Max(0, totalAmount - discountAmount);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        // Reload để lấy đầy đủ navigation properties
        return await GetTournamentDetailAsync(tournamentId, userId, false);
      }
      catch (Exception)
      {
        await transaction.RollbackAsync();
        throw;
      }
    }

    /// <summary>Maps tournament entity to TournamentDto.</summary>
    private static TournamentDto MapToTournamentDto(Tournament t)
    {
      return new TournamentDto
      {
        TournamentId = t.TournamentId,
        TournamentName = t.TournamentName,
        Description = t.Description,
        UserId = t.UserId,
        CustomerName = t.User?.FullName ?? $"User #{t.UserId}",
        TotalAmount = t.TotalAmount,
        Status = t.Status,
        CreatedAt = t.CreatedAt,
        Bookings = t.Bookings?.Select(MapToDto).ToList() ?? new List<BookingDto>()
      };
    }
  }
}
