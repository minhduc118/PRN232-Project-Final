using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Promotion;

namespace SportCourtManagent_Server.Services.Interfaces
{
  public interface IPromotionService
  {
    /// <summary>Gets all promotions asynchronous.</summary>
    Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync();

    /// <summary>Gets paged promotions with filtering asynchronous.</summary>
    Task<PagedResult<PromotionDto>> GetPagedPromotionsAsync(PromotionFilterParams filter);

    /// <summary>Gets a promotion by id asynchronous.</summary>
    Task<PromotionDto?> GetPromotionByIdAsync(int id);

    /// <summary>Creates a new promotion asynchronous.</summary>
    Task<PromotionDto> CreatePromotionAsync(CreatePromotionRequest request);

    /// <summary>Updates an existing promotion asynchronous.</summary>
    Task<PromotionDto?> UpdatePromotionAsync(int id, UpdatePromotionRequest request);

    /// <summary>Deletes a promotion asynchronous.</summary>
    Task<bool> DeletePromotionAsync(int id);

    /// <summary>Validates coupon and calculates discounts asynchronous.</summary>
    Task<ValidateCouponResponse> ValidateCouponAsync(ValidateCouponRequest request);
  }
}
