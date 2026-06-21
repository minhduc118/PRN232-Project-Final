using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Reviews;
using SportCourtManagerment.Repositories.Interfaces;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Services.Implementations;

/// <summary>
/// Review business logic: paginated court reviews and rating aggregation.
/// </summary>
public class ReviewService : IReviewService
{
  private readonly IReviewRepository _reviewRepo;

  public ReviewService(IReviewRepository reviewRepo)
  {
    _reviewRepo = reviewRepo;
  }

  /// <inheritdoc/>
  public async Task<PagedResult<ReviewDto>> GetCourtReviewsAsync(
    int courtId, int pageNumber, int pageSize)
  {
    var query = _reviewRepo.GetReviewsByCourtQueryable(courtId);

    var totalCount = await query.CountAsync();

    var items = await query
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .Select(r => new ReviewDto
      {
        ReviewId      = r.ReviewId,
        BookingId     = r.BookingId,
        CourtId       = r.CourtId,
        UserFullName  = r.User.FullName,
        UserAvatarUrl = r.User.AvatarUrl,
        Rating        = r.Rating,
        Comment       = r.Comment,
        ImageUrl      = r.ImageUrl,
        AdminReply    = r.AdminReply,
        RepliedAt     = r.RepliedAt,
        CreatedAt     = r.CreatedAt,
      })
      .ToListAsync();

    return new PagedResult<ReviewDto>
    {
      Items      = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize   = pageSize,
    };
  }

  /// <inheritdoc/>
  public async Task<CourtReviewSummaryDto> GetCourtReviewSummaryAsync(int courtId)
  {
    var (avgRating, totalCount, distribution) =
      await _reviewRepo.GetCourtRatingSummaryAsync(courtId);

    return new CourtReviewSummaryDto
    {
      AverageRating      = avgRating,
      TotalReviews       = totalCount,
      RatingDistribution = distribution,
    };
  }

  /// <inheritdoc/>
  public IQueryable<ReviewDto> GetReviewsODataQueryable(int? courtId = null)
  {
    var query = courtId.HasValue
      ? _reviewRepo.GetReviewsByCourtQueryable(courtId.Value)
      : _reviewRepo.FindBy(r => r.IsVisible);

    return query.Select(r => new ReviewDto
    {
      ReviewId      = r.ReviewId,
      BookingId     = r.BookingId,
      CourtId       = r.CourtId,
      UserFullName  = r.User.FullName,
      UserAvatarUrl = r.User.AvatarUrl,
      Rating        = r.Rating,
      Comment       = r.Comment,
      ImageUrl      = r.ImageUrl,
      AdminReply    = r.AdminReply,
      RepliedAt     = r.RepliedAt,
      CreatedAt     = r.CreatedAt,
    });
  }
}
