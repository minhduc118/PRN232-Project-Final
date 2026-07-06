using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Promotions;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.Services.Promotions;

public class PromotionService : IPromotionService
{
    private readonly ApplicationDbContext _context;

    public PromotionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync()
    {
        return await _context.Promotions
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PromotionDto
            {
                PromotionId = p.PromotionId,
                PromoCode = p.PromoCode,
                PromoName = p.PromoName,
                Description = p.Description,
                DiscountType = p.DiscountType.ToString(),
                DiscountValue = p.DiscountValue,
                MinOrderAmount = p.MinOrderAmount,
                MaxDiscount = p.MaxDiscount,
                UsageLimit = p.UsageLimit,
                UsedCount = p.UsedCount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PromotionDto?> GetPromotionByIdAsync(int id)
    {
        var p = await _context.Promotions.FindAsync(id);
        if (p == null) return null;

        return new PromotionDto
        {
            PromotionId = p.PromotionId,
            PromoCode = p.PromoCode,
            PromoName = p.PromoName,
            Description = p.Description,
            DiscountType = p.DiscountType.ToString(),
            DiscountValue = p.DiscountValue,
            MinOrderAmount = p.MinOrderAmount,
            MaxDiscount = p.MaxDiscount,
            UsageLimit = p.UsageLimit,
            UsedCount = p.UsedCount,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        };
    }

    public async Task<ApiResponse<PromotionDto>> CreatePromotionAsync(CreatePromotionDto dto)
    {
        // Validation: Dates
        if (dto.EndDate < dto.StartDate)
        {
            return ApiResponse<PromotionDto>.Fail("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }

        // Validation: Rules
        if (dto.DiscountType == DiscountType.Percent)
        {
            if (dto.DiscountValue < 1 || dto.DiscountValue > 100)
                return ApiResponse<PromotionDto>.Fail("Phần trăm giảm phải từ 1 đến 100.");
            if (!dto.MaxDiscount.HasValue || dto.MaxDiscount.Value <= 0)
                return ApiResponse<PromotionDto>.Fail("Phải nhập số tiền giảm tối đa khi chọn giảm theo phần trăm.");
        }
        else if (dto.DiscountType == DiscountType.FixedAmount)
        {
            if (dto.DiscountValue <= 0)
                return ApiResponse<PromotionDto>.Fail("Số tiền giảm phải lớn hơn 0.");
        }

        if (dto.UsageLimit.HasValue && dto.UsageLimit.Value <= 0)
        {
            return ApiResponse<PromotionDto>.Fail("Giới hạn số lượng phải lớn hơn 0.");
        }

        if (dto.MinOrderAmount < 0)
        {
            return ApiResponse<PromotionDto>.Fail("Đơn hàng tối thiểu không được âm.");
        }

        // Check Unique PromoCode
        var exists = await _context.Promotions.AnyAsync(p => p.PromoCode == dto.PromoCode);
        if (exists)
        {
            return ApiResponse<PromotionDto>.Fail($"Mã khuyến mãi '{dto.PromoCode}' đã tồn tại.");
        }

        var promotion = new Promotion
        {
            PromoCode = dto.PromoCode.ToUpper(),
            PromoName = dto.PromoName,
            Description = dto.Description,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MinOrderAmount = dto.MinOrderAmount,
            MaxDiscount = dto.DiscountType == DiscountType.Percent ? dto.MaxDiscount : null,
            UsageLimit = dto.UsageLimit,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            UsedCount = 0
        };

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();

        var createdDto = await GetPromotionByIdAsync(promotion.PromotionId);
        return ApiResponse<PromotionDto>.Ok(createdDto!, "Tạo mã khuyến mãi thành công.");
    }

    public async Task<ApiResponse<PromotionDto>> UpdatePromotionAsync(int id, UpdatePromotionDto dto)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
            return ApiResponse<PromotionDto>.Fail("Không tìm thấy mã khuyến mãi.");

        // Validation: Dates
        if (dto.EndDate < dto.StartDate)
        {
            return ApiResponse<PromotionDto>.Fail("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }

        // Validation: Rules
        if (dto.DiscountType == DiscountType.Percent)
        {
            if (dto.DiscountValue < 1 || dto.DiscountValue > 100)
                return ApiResponse<PromotionDto>.Fail("Phần trăm giảm phải từ 1 đến 100.");
            if (!dto.MaxDiscount.HasValue || dto.MaxDiscount.Value <= 0)
                return ApiResponse<PromotionDto>.Fail("Phải nhập số tiền giảm tối đa khi chọn giảm theo phần trăm.");
        }
        else if (dto.DiscountType == DiscountType.FixedAmount)
        {
            if (dto.DiscountValue <= 0)
                return ApiResponse<PromotionDto>.Fail("Số tiền giảm phải lớn hơn 0.");
        }

        if (dto.UsageLimit.HasValue && dto.UsageLimit.Value <= 0)
        {
            return ApiResponse<PromotionDto>.Fail("Giới hạn số lượng phải lớn hơn 0.");
        }

        if (dto.MinOrderAmount < 0)
        {
            return ApiResponse<PromotionDto>.Fail("Đơn hàng tối thiểu không được âm.");
        }

        promotion.PromoName = dto.PromoName;
        promotion.Description = dto.Description;
        promotion.DiscountType = dto.DiscountType;
        promotion.DiscountValue = dto.DiscountValue;
        promotion.MinOrderAmount = dto.MinOrderAmount;
        promotion.MaxDiscount = dto.DiscountType == DiscountType.Percent ? dto.MaxDiscount : null;
        promotion.UsageLimit = dto.UsageLimit;
        promotion.StartDate = dto.StartDate;
        promotion.EndDate = dto.EndDate;
        promotion.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        var updatedDto = await GetPromotionByIdAsync(promotion.PromotionId);
        return ApiResponse<PromotionDto>.Ok(updatedDto!, "Cập nhật mã khuyến mãi thành công.");
    }

    public async Task<ApiResponse<bool>> DeletePromotionAsync(int id)
    {
        var promotion = await _context.Promotions.Include(p => p.Bookings).FirstOrDefaultAsync(p => p.PromotionId == id);
        if (promotion == null)
            return ApiResponse<bool>.Fail("Không tìm thấy mã khuyến mãi.");

        if (promotion.Bookings.Any())
        {
            return ApiResponse<bool>.Fail("Không thể xóa mã khuyến mãi đã được sử dụng. Vui lòng vô hiệu hóa thay vì xóa.");
        }

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Xóa mã khuyến mãi thành công.");
    }
}
