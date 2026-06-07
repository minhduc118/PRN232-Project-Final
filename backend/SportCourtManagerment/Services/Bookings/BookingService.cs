using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Bookings;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.Services.Bookings;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookingAdminDto>> GetAdminBookingsAsync(DateOnly? date, int? courtTypeId, string? status)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Court)
            .Include(b => b.TimeSlot)
            .AsQueryable();

        if (date.HasValue)
        {
            query = query.Where(b => b.BookingDate == date.Value);
        }

        if (courtTypeId.HasValue)
        {
            query = query.Where(b => b.Court.CourtTypeId == courtTypeId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(b => b.Status == parsedStatus);
        }

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookingAdminDto
            {
                BookingId = b.BookingId,
                BookingCode = b.BookingCode,
                UserId = b.UserId,
                CustomerName = b.User.FullName,
                CustomerPhone = b.User.Phone ?? "",
                CourtId = b.CourtId,
                CourtName = b.Court.CourtName,
                CourtTypeId = b.Court.CourtTypeId,
                SlotId = b.SlotId,
                SlotName = b.TimeSlot.SlotName,
                BookingDate = b.BookingDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                SubTotal = b.SubTotal,
                DiscountAmount = b.DiscountAmount,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                Note = b.Note,
                CancelReason = b.CancelReason,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<BookingAdminDto?> GetBookingByIdAsync(int id)
    {
        var b = await _context.Bookings
            .Include(bk => bk.User)
            .Include(bk => bk.Court)
            .Include(bk => bk.TimeSlot)
            .FirstOrDefaultAsync(bk => bk.BookingId == id);
            
        if (b == null) return null;

        return new BookingAdminDto
        {
            BookingId = b.BookingId,
            BookingCode = b.BookingCode,
            UserId = b.UserId,
            CustomerName = b.User.FullName,
            CustomerPhone = b.User.Phone ?? "",
            CourtId = b.CourtId,
            CourtName = b.Court.CourtName,
            CourtTypeId = b.Court.CourtTypeId,
            SlotId = b.SlotId,
            SlotName = b.TimeSlot.SlotName,
            BookingDate = b.BookingDate,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            SubTotal = b.SubTotal,
            DiscountAmount = b.DiscountAmount,
            TotalAmount = b.TotalAmount,
            Status = b.Status,
            Note = b.Note,
            CancelReason = b.CancelReason,
            CreatedAt = b.CreatedAt
        };
    }

    public async Task<ApiResponse<BookingAdminDto>> CreateBookingFromAdminAsync(CreateBookingAdminDto dto)
    {
        // 1. Validate booking date — không cho đặt ngày quá khứ
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (dto.BookingDate < today)
        {
            return ApiResponse<BookingAdminDto>.Fail("Không thể đặt sân cho ngày đã qua.");
        }

        // 2. Validate Time
        if (dto.EndTime <= dto.StartTime)
        {
            return ApiResponse<BookingAdminDto>.Fail("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
        }

        // 3. Check overlap by time (not just SlotId)
        var overlap = await _context.Bookings.AnyAsync(b => 
            b.CourtId == dto.CourtId && 
            b.BookingDate == dto.BookingDate && 
            b.StartTime < dto.EndTime && 
            b.EndTime > dto.StartTime &&
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
            
        if (overlap)
        {
            return ApiResponse<BookingAdminDto>.Fail("Lịch đã bị trùng. Khung giờ này trên sân đã có người đặt.");
        }

        // 3. Calculate Price
        var court = await _context.Courts.FindAsync(dto.CourtId);
        if (court == null) return ApiResponse<BookingAdminDto>.Fail("Không tìm thấy sân.");
        
        // Calculate duration in hours
        var duration = (dto.EndTime - dto.StartTime).TotalHours;
        decimal subTotal = court.PricePerHour * (decimal)duration;
        decimal discountAmount = 0m;
        Promotion? appliedPromo = null;

        if (!string.IsNullOrEmpty(dto.PromotionCode))
        {
            appliedPromo = await _context.Promotions
                .FirstOrDefaultAsync(p => p.PromoCode == dto.PromotionCode && p.IsActive);

            if (appliedPromo != null && appliedPromo.StartDate <= DateTime.Now && appliedPromo.EndDate >= DateTime.Now)
            {
                if (subTotal >= appliedPromo.MinOrderAmount)
                {
                    if (appliedPromo.DiscountType == DiscountType.Percent)
                    {
                        var calculatedDiscount = subTotal * (appliedPromo.DiscountValue / 100);
                        if (appliedPromo.MaxDiscount.HasValue && calculatedDiscount > appliedPromo.MaxDiscount.Value)
                            discountAmount = appliedPromo.MaxDiscount.Value;
                        else
                            discountAmount = calculatedDiscount;
                    }
                    else
                    {
                        discountAmount = appliedPromo.DiscountValue;
                    }
                }
            }
            else
            {
                return ApiResponse<BookingAdminDto>.Fail("Mã khuyến mãi không hợp lệ hoặc đã hết hạn.");
            }
        }

        decimal totalAmount = Math.Max(0, subTotal - discountAmount);

        // 4. Create Booking
        var booking = new Booking
        {
            BookingCode = $"BK{DateTime.Now:yyyyMMddHHmmss}",
            UserId = dto.UserId,
            CourtId = dto.CourtId,
            SlotId = dto.SlotId,
            BookingDate = dto.BookingDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            Status = dto.PaymentStatus == "Success" ? BookingStatus.Confirmed : BookingStatus.Pending,
            PromotionId = appliedPromo?.PromotionId,
            Note = dto.Note,
            CreatedAt = DateTime.Now
        };

        _context.Bookings.Add(booking);
        
        // If Promo applied, increase UsedCount
        if (appliedPromo != null)
        {
            appliedPromo.UsedCount++;
        }

        // 5. Create Payment if Success
        if (dto.PaymentStatus == "Success" && Enum.TryParse<PaymentMethod>(dto.PaymentMethod, out var pMethod))
        {
            var payment = new Payment
            {
                Booking = booking,
                Amount = totalAmount,
                PaymentMethod = pMethod,
                Status = PaymentStatus.Success,
                TransactionId = $"TX_{DateTime.Now.Ticks}",
                CreatedAt = DateTime.Now
            };
            _context.Payments.Add(payment);
        }

        await _context.SaveChangesAsync();

        var resultDto = await GetBookingByIdAsync(booking.BookingId);
        return ApiResponse<BookingAdminDto>.Ok(resultDto!, "Tạo đơn đặt sân thành công.");
    }

    public async Task<ApiResponse<BookingAdminDto>> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return ApiResponse<BookingAdminDto>.Fail("Không tìm thấy đơn đặt sân.");

        // Business Rule: Transition matrix
        // Cancelled  → không cho chuyển sang bất kỳ trạng thái nào (đơn hủy là cuối cùng)
        // Completed  → chỉ cho chuyển về Confirmed (admin sửa nhầm), không cho hủy
        // Pending    → Confirmed | Cancelled
        // Confirmed  → Completed | Cancelled
        if (booking.Status == BookingStatus.Cancelled)
        {
            return ApiResponse<BookingAdminDto>.Fail("Đơn đã hủy không thể thay đổi trạng thái.");
        }

        if (booking.Status == BookingStatus.Completed && dto.Status != BookingStatus.Confirmed)
        {
            return ApiResponse<BookingAdminDto>.Fail(
                "Đơn đã hoàn thành chỉ có thể hoàn tác về 'Đã xác nhận' nếu admin nhập nhầm.");
        }

        if (dto.Status == BookingStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancelReason))
                return ApiResponse<BookingAdminDto>.Fail("Bắt buộc phải nhập lý do khi hủy đơn.");

            booking.CancelReason = dto.CancelReason;
            booking.CancelledAt = DateTime.Now;
        }

        booking.Status = dto.Status;
        booking.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        var resultDto = await GetBookingByIdAsync(booking.BookingId);
        return ApiResponse<BookingAdminDto>.Ok(resultDto!, "Cập nhật trạng thái thành công.");
    }
}
