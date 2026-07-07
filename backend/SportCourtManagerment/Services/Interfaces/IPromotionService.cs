using SportCourtManagerment.DTOs.Promotions;

namespace SportCourtManagerment.Services.Interfaces;

/// <summary>
/// Business logic for customer-visible promotion queries.
/// </summary>
public interface IPromotionService
{
  /// <summary>Returns all currently active and valid promotions.</summary>
  Task<List<PromotionDto>> GetActivePromotionsAsync();
}
