using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Promotion;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
  public class PromotionService : IPromotionService
  {
    private readonly IPromotionRepository _promoRepo;

    public PromotionService(IPromotionRepository promoRepo)
    {
      _promoRepo = promoRepo ?? throw new ArgumentNullException(nameof(promoRepo));
    }

    /// <summary>Gets all promotions asynchronous.</summary>
    public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync()
    {
      var promos = await _promoRepo.GetAllAsync();
      return promos.Select(MapToDto).ToList();
    }

    /// <summary>Gets a promotion by id asynchronous.</summary>
    public async Task<PromotionDto?> GetPromotionByIdAsync(int id)
    {
      var promo = await _promoRepo.GetByIdAsync(id);
      return promo == null ? null : MapToDto(promo);
    }

    /// <summary>Creates a new promotion asynchronous.</summary>
    public async Task<PromotionDto> CreatePromotionAsync(CreatePromotionRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));

      var existing = await _promoRepo.GetByCodeAsync(request.PromoCode);
      if (existing != null)
      {
        throw new InvalidOperationException($"Promotion code {request.PromoCode} already exists.");
      }

      var promo = new Promotion
      {
        PromoCode = request.PromoCode.ToUpper(),
        PromoName = request.PromoName,
        Description = request.Description,
        DiscountType = request.DiscountType,
        DiscountValue = request.DiscountValue,
        MinOrderAmount = request.MinOrderAmount,
        MaxDiscount = request.MaxDiscount,
        UsageLimit = request.UsageLimit,
        UsedCount = 0,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        IsActive = request.IsActive,
        CreatedAt = DateTime.UtcNow
      };

      await _promoRepo.AddAsync(promo);
      return MapToDto(promo);
    }

    /// <summary>Updates an existing promotion asynchronous.</summary>
    public async Task<PromotionDto?> UpdatePromotionAsync(int id, UpdatePromotionRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));

      var promo = await _promoRepo.GetByIdAsync(id);
      if (promo == null) return null;

      promo.PromoName = request.PromoName;
      promo.Description = request.Description;
      promo.DiscountType = request.DiscountType;
      promo.DiscountValue = request.DiscountValue;
      promo.MinOrderAmount = request.MinOrderAmount;
      promo.MaxDiscount = request.MaxDiscount;
      promo.UsageLimit = request.UsageLimit;
      promo.StartDate = request.StartDate;
      promo.EndDate = request.EndDate;
      promo.IsActive = request.IsActive;

      await _promoRepo.UpdateAsync(promo);
      return MapToDto(promo);
    }

    /// <summary>Deletes a promotion asynchronous.</summary>
    public async Task<bool> DeletePromotionAsync(int id)
    {
      var promo = await _promoRepo.GetByIdAsync(id);
      if (promo == null) return false;

      await _promoRepo.DeleteAsync(id);
      return true;
    }

    /// <summary>Validates coupon and calculates discounts asynchronous.</summary>
    public async Task<ValidateCouponResponse> ValidateCouponAsync(ValidateCouponRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));

      var promo = await _promoRepo.GetByCodeAsync(request.PromoCode);
      if (promo == null)
      {
        return new ValidateCouponResponse { Valid = false, Message = "Mã giảm giá không tồn tại." };
      }

      return CalculateCouponDiscount(promo, request.OrderAmount);
    }

    /// <summary>Helper method to calculate discount amount.</summary>
    private static ValidateCouponResponse CalculateCouponDiscount(Promotion promo, decimal orderAmount)
    {
      var now = DateTime.UtcNow;
      if (!promo.IsActive || now < promo.StartDate || now > promo.EndDate)
      {
        return new ValidateCouponResponse { Valid = false, Message = "Mã giảm giá đã hết hạn hoặc đang bị khóa." };
      }

      if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
      {
        return new ValidateCouponResponse { Valid = false, Message = "Mã giảm giá đã hết lượt sử dụng." };
      }

      if (orderAmount < promo.MinOrderAmount)
      {
        return new ValidateCouponResponse { Valid = false, Message = $"Đơn hàng tối thiểu phải từ {promo.MinOrderAmount:N0}đ." };
      }

      decimal discount = 0;
      if (promo.DiscountType == DiscountType.Percent)
      {
        discount = orderAmount * (promo.DiscountValue / 100m);
        if (promo.MaxDiscount.HasValue && discount > promo.MaxDiscount.Value)
        {
          discount = promo.MaxDiscount.Value;
        }
      }
      else
      {
        discount = promo.DiscountValue;
      }

      if (discount > orderAmount) discount = orderAmount;

      return new ValidateCouponResponse
      {
        Valid = true,
        PromoCode = promo.PromoCode,
        PromoName = promo.PromoName,
        DiscountType = promo.DiscountType.ToString(),
        DiscountValue = promo.DiscountValue,
        DiscountAmount = discount,
        FinalAmount = orderAmount - discount
      };
    }

    /// <summary>Maps entity to DTO.</summary>
    private static PromotionDto MapToDto(Promotion p)
    {
      return new PromotionDto
      {
        PromotionId = p.PromotionId,
        PromoCode = p.PromoCode,
        PromoName = p.PromoName,
        Description = p.Description,
        DiscountType = p.DiscountType,
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
  }
}
