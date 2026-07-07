using SportCourtManagerment.Models;

namespace SportCourtManagerment.Repositories.Interfaces;

/// <summary>
/// Review-specific repository with court-scoped queries and rating aggregation.
/// </summary>
public interface IReviewRepository : IGenericRepository<Review>
{
  /// <summary>
  /// Returns a queryable of visible reviews for a specific court,
  /// including the reviewing User navigation.
  /// </summary>
  IQueryable<Review> GetReviewsByCourtQueryable(int courtId);

  /// <summary>
  /// Returns aggregated rating statistics for a court:
  /// average rating, total count, and per-star distribution.
  /// </summary>
  Task<(double avgRating, int totalCount, Dictionary<int, int> distribution)>
    GetCourtRatingSummaryAsync(int courtId);
}
