using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Booking;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
  public class BookingManagementService : IBookingManagementService
  {
    private readonly IBookingRepository _bookingRepo;
    private readonly IPromotionRepository _promoRepo;
    private readonly AppDbContext _context;

    public BookingManagementService(IBookingRepository bookingRepo, IPromotionRepository promoRepo, AppDbContext context)
    {
      _bookingRepo = bookingRepo ?? throw new ArgumentNullException(nameof(bookingRepo));
      _promoRepo = promoRepo ?? throw new ArgumentNullException(nameof(promoRepo));
      _context = context ?? throw new ArgumentNullException(nameof(context));
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
      return MapToDto(await _bookingRepo.GetDetailAsync(booking.BookingId) ?? booking);
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
  }
}
