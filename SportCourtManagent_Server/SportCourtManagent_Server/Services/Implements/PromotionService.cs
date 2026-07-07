using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Promotion;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
  public class PromotionService : IPromotionService
  {
    private readonly IPromotionRepository _promoRepo;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "PromotionsList";

    public PromotionService(IPromotionRepository promoRepo, IMemoryCache cache)
    {
      _promoRepo = promoRepo ?? throw new ArgumentNullException(nameof(promoRepo));
      _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>Gets all promotions asynchronous with caching.</summary>
    public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync()
    {
      if (!_cache.TryGetValue(CacheKey, out List<PromotionDto>? promos) || promos == null)
      {
        var list = await _promoRepo.GetAllAsync();
        promos = list.Select(MapToDto).ToList();
        _cache.Set(CacheKey, promos, TimeSpan.FromMinutes(5));
      }
      return promos;
    }

    /// <summary>Gets paged promotions with filtering asynchronous.</summary>
    public async Task<PagedResult<PromotionDto>> GetPagedPromotionsAsync(PromotionFilterParams filter)
    {
      var allPromos = await GetAllPromotionsAsync();
      var query = FilterPromotions(allPromos, filter);
      var total = query.Count();
      var items = query
        .Skip((filter.PageNumber - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .ToList();

      return new PagedResult<PromotionDto>
      {
        Items = items,
        TotalCount = total,
        PageNumber = filter.PageNumber,
        PageSize = filter.PageSize
      };
    }

    /// <summary>Filters promotions list in memory.</summary>
    private static IEnumerable<PromotionDto> FilterPromotions(IEnumerable<PromotionDto> list, PromotionFilterParams filter)
    {
      var query = list;
      if (!string.IsNullOrWhiteSpace(filter.Keyword))
      {
        var kw = filter.Keyword.Trim().ToLower();
        query = query.Where(p => p.PromoCode.ToLower().Contains(kw) || p.PromoName.ToLower().Contains(kw));
      }
      if (filter.IsActive.HasValue)
      {
        query = query.Where(p => p.IsActive == filter.IsActive.Value);
      }
      return query;
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

      ValidatePromotionRules(request.DiscountType, request.DiscountValue, request.MinOrderAmount, request.MaxDiscount, request.UsageLimit, request.StartDate, request.EndDate);

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
      _cache.Remove(CacheKey);
      return MapToDto(promo);
    }

    /// <summary>Updates an existing promotion asynchronous.</summary>
    public async Task<PromotionDto?> UpdatePromotionAsync(int id, UpdatePromotionRequest request)
    {
      if (request == null) throw new ArgumentNullException(nameof(request));

      ValidatePromotionRules(request.DiscountType, request.DiscountValue, request.MinOrderAmount, request.MaxDiscount, request.UsageLimit, request.StartDate, request.EndDate);

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
      _cache.Remove(CacheKey);
      return MapToDto(promo);
    }

    /// <summary>Deletes a promotion asynchronous.</summary>
    public async Task<bool> DeletePromotionAsync(int id)
    {
      var promo = await _promoRepo.GetByIdAsync(id);
      if (promo == null) return false;

      await _promoRepo.DeleteAsync(id);
      _cache.Remove(CacheKey);
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

    private static void ValidatePromotionRules(DiscountType discountType, decimal discountValue, decimal minOrderAmount, decimal? maxDiscount, int? usageLimit, DateTime startDate, DateTime endDate)
    {
      if (endDate < startDate)
        throw new ArgumentException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
      if (discountValue <= 0)
        throw new ArgumentException("Giá trị giảm giá phải lớn hơn 0.");
      if (discountType == DiscountType.Percent && discountValue > 100)
        throw new ArgumentException("Khuyến mãi theo phần trăm không được vượt quá 100%.");
      if (minOrderAmount < 0)
        throw new ArgumentException("Giá trị đơn hàng tối thiểu không được âm.");
      if (maxDiscount.HasValue && maxDiscount.Value <= 0)
        throw new ArgumentException("Số tiền giảm tối đa phải lớn hơn 0.");
      if (usageLimit.HasValue && usageLimit.Value <= 0)
        throw new ArgumentException("Giới hạn sử dụng phải lớn hơn 0.");
    }
  }
}
