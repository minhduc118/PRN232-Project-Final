using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Review;

namespace SportCourtManagent_Server.Services.Interfaces
{
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
        /// </summary>
        Task<(ReviewDto? review, string? error)> CreateReviewAsync(
            int courtId, int userId, CreateReviewDto dto);

        /// <summary>Returns an IQueryable of reviews for OData endpoint.</summary>
        IQueryable<ReviewDto> GetReviewsODataQueryable(int? courtId = null);
    }
}
