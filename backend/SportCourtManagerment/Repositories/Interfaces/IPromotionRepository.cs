using SportCourtManagerment.Models;

namespace SportCourtManagerment.Repositories.Interfaces;

/// <summary>
/// Promotion-specific repository for active promotion queries.
/// </summary>
public interface IPromotionRepository : IGenericRepository<Promotion>
{
  /// <summary>
  /// Returns a queryable of currently active and valid promotions
  /// (IsActive = true, within StartDate–EndDate range, not exhausted).
  /// </summary>
  IQueryable<Promotion> GetActivePromotionsQueryable();
}
