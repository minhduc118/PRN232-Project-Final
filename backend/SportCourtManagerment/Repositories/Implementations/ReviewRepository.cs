using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.Models;
using SportCourtManagerment.Repositories.Interfaces;

namespace SportCourtManagerment.Repositories.Implementations;

/// <summary>
/// Review repository with court-scoped queries and aggregation logic.
/// </summary>
public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
  public ReviewRepository(ApplicationDbContext context) : base(context) { }

  /// <inheritdoc/>
  public IQueryable<Review> GetReviewsByCourtQueryable(int courtId)
  {
    return _context.Reviews
      .AsNoTracking()
      .Where(r => r.CourtId == courtId && r.IsVisible)
      .Include(r => r.User)
      .OrderByDescending(r => r.CreatedAt);
  }

  /// <inheritdoc/>
  public async Task<(double avgRating, int totalCount, Dictionary<int, int> distribution)>
    GetCourtRatingSummaryAsync(int courtId)
  {
    var reviews = await _context.Reviews
      .AsNoTracking()
      .Where(r => r.CourtId == courtId && r.IsVisible)
      .Select(r => (int)r.Rating)
      .ToListAsync();

    if (reviews.Count == 0)
      return (0, 0, new Dictionary<int, int>
      {
        { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
      });

    var avgRating    = reviews.Average();
    var distribution = Enumerable.Range(1, 5)
      .ToDictionary(star => star, star => reviews.Count(r => r == star));

    return (Math.Round(avgRating, 1), reviews.Count, distribution);
  }
}
