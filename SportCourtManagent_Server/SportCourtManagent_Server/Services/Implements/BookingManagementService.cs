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

    public BookingManagementService(
      IBookingRepository bookingRepo,
      IPromotionRepository promoRepo,
      AppDbContext context,
      IMemoryCache cache,
      ITournamentLockManager lockManager,
      IHubContext<SlotStatusHub> hubContext)

    {
      _bookingRepo = bookingRepo ?? throw new ArgumentNullException(nameof(bookingRepo));
      _promoRepo = promoRepo ?? throw new ArgumentNullException(nameof(promoRepo));
      _context = context ?? throw new ArgumentNullException(nameof(context));
      _cache = cache ?? throw new ArgumentNullException(nameof(cache));
      _lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
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

      // Check if court supports this slot
      var hasPricing = await _context.CourtPricings.AnyAsync(cp => cp.CourtId == request.CourtId && cp.SlotId == request.SlotId);
      if (!hasPricing)
      {
          throw new ArgumentException("Sân đấu này không mở cửa hoạt động trong khung giờ đã chọn.");
      }

      // Check for duplicate slot booking & apply lazy expiration
      var existingBookings = await _context.Bookings.Where(b =>
        b.CourtId == request.CourtId
        && b.SlotId == request.SlotId
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
          throw new ArgumentException($"Khung giờ {slot.SlotName} ngày {request.BookingDate:dd/MM/yyyy} đã có người đặt hoặc đang giữ chỗ chờ thanh toán.");
        }
      }
      if (existingBookings.Any(b => b.Status == BookingStatus.Cancelled))
      {
        await _context.SaveChangesAsync();
      }

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
        CreatedAt = DateTime.UtcNow,
        ExpiredAt = DateTime.UtcNow.AddMinutes(10)
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

      // Check if court supports this slot
      var hasPricing = await _context.CourtPricings.AnyAsync(cp => cp.CourtId == request.CourtId && cp.SlotId == request.SlotId);
      if (!hasPricing)
      {
          throw new ArgumentException("Sân đấu này không mở cửa hoạt động trong khung giờ đã chọn.");
      }

      // Generate all target dates from StartDate to EndDate matching DaysOfWeek
      var allDates = new List<DateTime>();
      for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
      {
        if (request.DaysOfWeek.Contains((int)date.DayOfWeek))
          allDates.Add(date);
      }

      if (!allDates.Any())
        throw new ArgumentException("Không có ngày nào phù hợp trong khoảng thời gian đã chọn.");

      // Batch check all conflicting dates & apply lazy expiration
      var existingBookings = await _context.Bookings
        .Where(b => b.CourtId == request.CourtId
                  && b.SlotId == request.SlotId
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
        return await ExecuteCreateTournamentTransactionAsync(userId, request);
      }
      finally
      {
        ReleaseTournamentLocks(acquiredPairs);
      }
    }

    /// <summary>Validates the tournament request.</summary>
    private static void ValidateCreateTournamentRequest(CreateTournamentRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));
      if (request.CourtSelections == null || !request.CourtSelections.Any())
        throw new ArgumentException("Giải đấu phải có ít nhất một sân được chọn.");
    }

    /// <summary>Gets unique court and date pairs sorted to prevent deadlock.</summary>
    private static List<(int CourtId, DateTime Date)> GetUniqueCourtDatePairs(CreateTournamentRequest request)
    {
      return request.CourtSelections
        .Select(c => (c.CourtId, request.BookingDate.Date))
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

    /// <summary>Executes tournament creation inside a database transaction.</summary>
    private async Task<TournamentDto> ExecuteCreateTournamentTransactionAsync(int userId, CreateTournamentRequest request)
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
          ExpiredAt = DateTime.UtcNow.AddMinutes(10)
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
    }

    /// <summary>Processes batch loading and creates tournament bookings without loop queries.</summary>
    private async Task<List<Booking>> ProcessTournamentBookingsAsync(int userId, int tournamentId, CreateTournamentRequest request)
    {
      var courtIds = request.CourtSelections.Select(c => c.CourtId).Distinct().ToList();
      var slotIds = request.CourtSelections.SelectMany(c => c.SlotIds ?? new List<int>()).Distinct().ToList();
      var serviceIds = request.Services?.Select(s => s.ServiceId).Distinct().ToList() ?? new List<int>();

      var slots = await _context.TimeSlots.Where(s => slotIds.Contains(s.SlotId)).ToDictionaryAsync(s => s.SlotId);
      var courts = await _context.Courts.Where(c => courtIds.Contains(c.CourtId)).ToDictionaryAsync(c => c.CourtId);
      var pricings = await _context.CourtPricings.Where(p => courtIds.Contains(p.CourtId) && slotIds.Contains(p.SlotId)).ToListAsync();
      var services = await _context.Services.Where(s => serviceIds.Contains(s.ServiceId)).ToDictionaryAsync(s => s.ServiceId);
      var activeBookings = await _context.Bookings.Where(b => courtIds.Contains(b.CourtId) && b.BookingDate.Date == request.BookingDate.Date && b.Status != BookingStatus.Cancelled).ToListAsync();

      var createdBookings = new List<Booking>();
      foreach (var sel in request.CourtSelections)
      {
        if (sel.SlotIds == null) continue;
        foreach (var slotId in sel.SlotIds)
        {
          var booking = BuildSingleTournamentBooking(userId, tournamentId, sel.CourtId, slotId, request, slots, courts, pricings, services, activeBookings);
          createdBookings.Add(booking);
        }
      }
      await _context.Bookings.AddRangeAsync(createdBookings);
      return createdBookings;
    }

    /// <summary>Builds a single booking entity and checks lazy expiration.</summary>
    private Booking BuildSingleTournamentBooking(
      int userId, int tournamentId, int courtId, int slotId, CreateTournamentRequest request,
      Dictionary<int, TimeSlot> slots, Dictionary<int, Court> courts, List<CourtPricing> pricings,
      Dictionary<int, Service> services, List<Booking> activeBookings)
    {
      if (!slots.TryGetValue(slotId, out var slot)) throw new ArgumentException($"Khung giờ {slotId} không hợp lệ.");
      CheckSlotConflictAndLazyExpire(courtId, slotId, request.BookingDate, slot.SlotName, activeBookings);

      decimal subTotal = CalculateBatchSubTotal(courtId, slot, request.Services, courts, pricings, services);
      var booking = new Booking
      {
        BookingCode = $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}",
        UserId = userId,
        CourtId = courtId,
        SlotId = slotId,
        BookingDate = request.BookingDate,
        StartTime = slot.StartTime,
        EndTime = slot.EndTime,
        SubTotal = subTotal,
        TotalAmount = subTotal,
        Status = BookingStatus.Pending,
        TournamentId = tournamentId,
        Note = request.Note,
        CreatedAt = DateTime.UtcNow,
        ExpiredAt = DateTime.UtcNow.AddMinutes(10)
      };
      AddBatchBookingServices(booking, request.Services, services);
      return booking;
    }

    /// <summary>Checks slot conflict and applies lazy expiration for pending timed-out bookings.</summary>
    private void CheckSlotConflictAndLazyExpire(int courtId, int slotId, DateTime date, string slotName, List<Booking> activeBookings)
    {
      var conflict = activeBookings.FirstOrDefault(b => b.CourtId == courtId && b.SlotId == slotId);
      if (conflict != null)
      {
        if (conflict.Status == BookingStatus.Pending && conflict.ExpiredAt.HasValue && conflict.ExpiredAt.Value < DateTime.UtcNow)
        {
          conflict.Status = BookingStatus.Cancelled;
          conflict.CancelReason = "Hết hạn thanh toán (TTL expired)";
          _context.Bookings.Update(conflict);
        }
        else
        {
          throw new ArgumentException($"Sân {courtId} vào khung giờ {slotName} ngày {date:dd/MM/yyyy} đã có người đặt hoặc đang giữ chỗ thanh toán.");
        }
      }
    }

    /// <summary>Calculates subtotal using batch loaded dictionaries.</summary>
    private static decimal CalculateBatchSubTotal(int courtId, TimeSlot slot, List<ServiceItemRequest>? reqServices, Dictionary<int, Court> courts, List<CourtPricing> pricings, Dictionary<int, Service> services)
    {
      var pricing = pricings.FirstOrDefault(p => p.CourtId == courtId && p.SlotId == slot.SlotId);
      decimal subTotal = pricing != null ? pricing.Price : (courts.TryGetValue(courtId, out var c) ? c.PricePerHour * (decimal)(slot.EndTime - slot.StartTime).TotalHours : 0);
      if (subTotal == 0 && courts.TryGetValue(courtId, out var court)) subTotal = court.PricePerHour;

      if (reqServices != null)
      {
        foreach (var item in reqServices.Where(x => x.Quantity > 0))
        {
          if (services.TryGetValue(item.ServiceId, out var s)) subTotal += s.Price * item.Quantity;
        }
      }
      return subTotal;
    }

    /// <summary>Adds booking services from batch dictionary.</summary>
    private static void AddBatchBookingServices(Booking booking, List<ServiceItemRequest>? reqServices, Dictionary<int, Service> services)
    {
      if (reqServices == null) return;
      foreach (var item in reqServices.Where(x => x.Quantity > 0))
      {
        if (services.TryGetValue(item.ServiceId, out var s))
        {
          booking.BookingServices.Add(new BookingService { ServiceId = item.ServiceId, Quantity = item.Quantity, TotalPrice = s.Price * item.Quantity });
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

    /// <summary>Applies smart diffing: cancels removed slots and adds newly selected slots.</summary>
    private async Task ApplyTournamentDiffingAsync(Tournament tournament, int userId, UpdateTournamentInfoRequest request)
    {
      var requestedPairs = new HashSet<(int CourtId, int SlotId)>();
      foreach (var sel in request.CourtSelections)
      {
        if (sel.SlotIds == null) continue;
        foreach (var slotId in sel.SlotIds) requestedPairs.Add((sel.CourtId, slotId));
      }

      foreach (var existing in tournament.Bookings)
      {
        if (!requestedPairs.Contains((existing.CourtId, existing.SlotId)) && existing.Status != BookingStatus.Cancelled)
        {
          existing.Status = BookingStatus.Cancelled;
          existing.CancelReason = "Bỏ ca khi sửa giải đấu";
        }
      }

      var existingActivePairs = new HashSet<(int CourtId, int SlotId)>(
        tournament.Bookings.Where(b => b.Status != BookingStatus.Cancelled).Select(b => (b.CourtId, b.SlotId)));

      var newBookings = new List<Booking>();
      foreach (var pair in requestedPairs)
      {
        if (!existingActivePairs.Contains(pair))
        {
          var existingConflicts = await _context.Bookings.Where(b =>
            b.CourtId == pair.CourtId
            && b.SlotId == pair.SlotId
            && b.BookingDate.Date == request.BookingDate.Date
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
              throw new ArgumentException($"Sân {pair.CourtId} vào khung giờ {pair.SlotId} ngày {request.BookingDate:dd/MM/yyyy} đã có người đặt.");
            }
          }

          var slot = await _context.TimeSlots.FindAsync(pair.SlotId) ?? throw new ArgumentException($"Khung giờ {pair.SlotId} không hợp lệ.");
          decimal subTotal = await CalculateSubTotalAsync(pair.CourtId, slot, request.Services);
          var booking = new Booking
          {
            BookingCode = $"TBK{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}",
            UserId = userId, CourtId = pair.CourtId, SlotId = pair.SlotId, BookingDate = request.BookingDate,
            StartTime = slot.StartTime, EndTime = slot.EndTime, SubTotal = subTotal, TotalAmount = subTotal,
            Status = BookingStatus.Pending, TournamentId = tournament.TournamentId, Note = request.Note,
            CreatedAt = DateTime.UtcNow, ExpiredAt = DateTime.UtcNow.AddMinutes(10)
          };
          await AddBookingServicesAsync(booking, request.Services);
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
      decimal subTotal = pricing != null ? pricing.Price : 0;
      if (subTotal == 0)
      {
        var court = await _context.Courts.FindAsync(courtId);
        decimal hours = (decimal)(slot.EndTime - slot.StartTime).TotalHours;
        subTotal = (court?.PricePerHour ?? 0) * (hours > 0 ? hours : 1);
      }
      if (services != null && services.Any())
      {
        var serviceIds = services.Select(s => s.ServiceId).ToList();
        var dbServices = await _context.Services.Where(s => serviceIds.Contains(s.ServiceId)).ToListAsync();
        foreach (var item in services.Where(x => x.Quantity > 0))
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
      var serviceIds = services.Select(s => s.ServiceId).ToList();
      var dbServices = await _context.Services.Where(s => serviceIds.Contains(s.ServiceId)).ToListAsync();
      foreach (var item in services.Where(x => x.Quantity > 0))
      {
        var s = dbServices.FirstOrDefault(x => x.ServiceId == item.ServiceId);
        if (s != null)
        {
          booking.BookingServices.Add(new BookingService { ServiceId = item.ServiceId, Quantity = item.Quantity, TotalPrice = s.Price * item.Quantity });
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

    /// <summary>Maps booking entity to DTO.</summary>
    private static BookingDto MapToDto(Booking b)
    {
      return new BookingDto
      {
        BookingId = b.BookingId, BookingCode = b.BookingCode, UserId = b.UserId,
        CustomerName = b.User?.FullName ?? $"User #{b.UserId}", CustomerPhone = b.User?.Phone,
        CourtId = b.CourtId, CourtName = b.Court?.CourtName ?? $"Court #{b.CourtId}",
        SlotId = b.SlotId, SlotName = b.TimeSlot?.SlotName ?? $"{b.StartTime:hh\\:mm} - {b.EndTime:hh\\:mm}",
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
          UnitPrice = bs.Service?.Price ?? 0,
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
        var dbTournaments = await GetBaseTournamentQuery().Where(t => t.Status == TournamentStatus.Paid || t.Status == TournamentStatus.Confirmed).OrderByDescending(t => t.CreatedAt).ToListAsync();
        allPublic = dbTournaments.Select(MapToPublicDto).ToList();
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

      decimal additionalAmount = 0;
      foreach (var kvp in serviceQuantities.Where(q => q.Value > 0))
      {
        var service = await _context.Services.FindAsync(kvp.Key);
        if (service == null) throw new ArgumentException($"Dịch vụ #{kvp.Key} không tồn tại.");

        if (service.StockQty < kvp.Value)
          throw new InvalidOperationException($"Số lượng hàng tồn kho không đủ cho dịch vụ '{service.ServiceName}'. Còn lại: {service.StockQty}, Yêu cầu: {kvp.Value}");

        // Deduct stock
        service.StockQty -= kvp.Value;
        _context.Services.Update(service);

        // Check if service already exists in booking
        var existingService = booking.BookingServices.FirstOrDefault(bs => bs.ServiceId == kvp.Key);
        if (existingService != null)
        {
          existingService.Quantity += kvp.Value;
          existingService.TotalPrice += service.Price * kvp.Value;
        }
        else
        {
          var bookingService = new BookingService
          {
            BookingId = bookingId,
            ServiceId = kvp.Key,
            Quantity = kvp.Value,
            TotalPrice = service.Price * kvp.Value
          };
          booking.BookingServices.Add(bookingService);
        }

        additionalAmount += service.Price * kvp.Value;
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

      await _bookingRepo.UpdateAsync(booking);
      return MapToDto(booking);
    }
  }
}
