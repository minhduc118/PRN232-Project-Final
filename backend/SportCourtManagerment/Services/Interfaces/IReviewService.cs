using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Reviews;

namespace SportCourtManagerment.Services.Interfaces;

/// <summary>
/// Business logic for court reviews and rating summaries.
/// </summary>
public interface IReviewService
{
  /// <summary>Returns paginated reviews for a specific court.</summary>
  Task<PagedResult<ReviewDto>> GetCourtReviewsAsync(int courtId, int pageNumber, int pageSize);

  /// <summary>Returns aggregated rating statistics for a court.</summary>
  Task<CourtReviewSummaryDto> GetCourtReviewSummaryAsync(int courtId);

  /// <summary>
  /// Creates a new review for a completed booking.
  /// Validates: booking exists, belongs to courtId, status = Completed,
  /// user owns the booking, and no existing review for that booking.
  /// </summary>
  Task<(ReviewDto? review, string? error)> CreateReviewAsync(
    int courtId, int userId, CreateReviewDto dto);

  /// <summary>Returns an IQueryable of reviews for OData endpoint.</summary>
  IQueryable<ReviewDto> GetReviewsODataQueryable(int? courtId = null);
}
