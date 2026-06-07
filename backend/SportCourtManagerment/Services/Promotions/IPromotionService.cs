using SportCourtManagerment.DTOs.Promotions;
using SportCourtManagerment.DTOs;

namespace SportCourtManagerment.Services.Promotions;

public interface IPromotionService
{
    Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync();
    Task<PromotionDto?> GetPromotionByIdAsync(int id);
    Task<ApiResponse<PromotionDto>> CreatePromotionAsync(CreatePromotionDto dto);
    Task<ApiResponse<PromotionDto>> UpdatePromotionAsync(int id, UpdatePromotionDto dto);
    Task<ApiResponse<bool>> DeletePromotionAsync(int id);
}
