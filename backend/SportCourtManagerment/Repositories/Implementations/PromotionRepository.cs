using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.Models;
using SportCourtManagerment.Repositories.Interfaces;

namespace SportCourtManagerment.Repositories.Implementations;

/// <summary>
/// Promotion repository filtering currently active and valid promotions.
/// </summary>
public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
{
  public PromotionRepository(ApplicationDbContext context) : base(context) { }

  /// <inheritdoc/>
  public IQueryable<Promotion> GetActivePromotionsQueryable()
  {
    var now = DateTime.UtcNow;
    return _context.Promotions
      .AsNoTracking()
      .Where(p => p.IsActive
                   && p.StartDate <= now
                   && p.EndDate >= now
                   && (p.UsageLimit == null || p.UsedCount < p.UsageLimit));
  }
}
