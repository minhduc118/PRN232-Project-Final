using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.DTOs.Promotions;
using SportCourtManagerment.Repositories.Interfaces;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Services.Implementations;

/// <summary>
/// Promotion business logic: list active promotions for customer display.
/// </summary>
public class PromotionService : IPromotionService
{
  private readonly IPromotionRepository _promoRepo;

  public PromotionService(IPromotionRepository promoRepo)
  {
    _promoRepo = promoRepo;
  }

  /// <inheritdoc/>
  public async Task<List<PromotionDto>> GetActivePromotionsAsync()
  {
    return await _promoRepo.GetActivePromotionsQueryable()
      .OrderByDescending(p => p.CreatedAt)
      .Select(p => new PromotionDto
      {
        PromotionId    = p.PromotionId,
        PromoCode      = p.PromoCode,
        PromoName      = p.PromoName,
        Description    = p.Description,
        DiscountType   = p.DiscountType.ToString(),
        DiscountValue  = p.DiscountValue,
        MinOrderAmount = p.MinOrderAmount,
        MaxDiscount    = p.MaxDiscount,
        StartDate      = p.StartDate,
        EndDate        = p.EndDate,
      })
      .ToListAsync();
  }
}
