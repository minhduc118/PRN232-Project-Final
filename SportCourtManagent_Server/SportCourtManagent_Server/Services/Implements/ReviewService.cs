using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Review;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IBookingRepository _bookingRepo;

        public ReviewService(IReviewRepository reviewRepo, IBookingRepository bookingRepo)
        {
            _reviewRepo = reviewRepo;
            _bookingRepo = bookingRepo;
        }

       
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
                    ReviewId = r.ReviewId,
                    BookingId = r.BookingId,
                    CourtId = r.CourtId,
                    UserFullName = r.User.FullName,
                    UserAvatarUrl = r.User.AvatarUrl,
                    Rating = r.Rating,
                    Comment = r.Comment,
                })
                .ToListAsync();

            return new PagedResult<ReviewDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
        }

        
        public async Task<CourtReviewSummaryDto> GetCourtReviewSummaryAsync(int courtId)
        {
            var (avgRating, totalCount, distribution) =
                await _reviewRepo.GetCourtRatingSummaryAsync(courtId);

            return new CourtReviewSummaryDto
            {
                AverageRating = avgRating,
                TotalReviews = totalCount,
                RatingDistribution = distribution,
            };
        }

        
        public async Task<(ReviewDto? review, string? error)> CreateReviewAsync(
            int courtId, int userId, CreateReviewDto dto)
        {
            // 1. Validate booking exists
            var booking = await _bookingRepo.GetDetailAsync(dto.BookingId);

            if (booking is null)
                return (null, "Booking không tồn tại.");

            // 2. Validate booking belongs to this court
            if (booking.CourtId != courtId)
                return (null, "Booking không thuộc sân này.");

            // 3. Validate user owns the booking
            if (booking.UserId != userId)
                return (null, "Bạn không có quyền đánh giá booking này.");

            // 4. Validate booking is completed
            if (booking.Status != BookingStatus.Completed)
                return (null, "Chỉ có thể đánh giá booking đã hoàn thành.");

            // 5. Validate no existing review for this booking (1:1)
            var reviewExists = await _reviewRepo.FindBy(r => r.BookingId == dto.BookingId).AnyAsync();
            if (reviewExists)
                return (null, "Booking này đã được đánh giá rồi.");

            // 6. Create review entity
            var review = new Review
            {
                BookingId = dto.BookingId,
                UserId = userId,
                CourtId = courtId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                IsVisible = true,
            };

            await _reviewRepo.AddAsync(review);
            await _reviewRepo.SaveChangesAsync();

            // 7. Reload with User navigation for response
            var createdReview = await _reviewRepo
                .FindBy(r => r.ReviewId == review.ReviewId)
                .Include(r => r.User)
                .FirstAsync();

            var reviewDto = new ReviewDto
            {
                ReviewId = createdReview.ReviewId,
                BookingId = createdReview.BookingId,
                CourtId = createdReview.CourtId,
                UserFullName = createdReview.User.FullName,
                UserAvatarUrl = createdReview.User.AvatarUrl,
                Rating = createdReview.Rating,
                Comment = createdReview.Comment,
            };

            return (reviewDto, null);
        }

        
        public IQueryable<ReviewDto> GetReviewsODataQueryable(int? courtId = null)
        {
            var query = courtId.HasValue
                ? _reviewRepo.GetReviewsByCourtQueryable(courtId.Value)
                : _reviewRepo.FindBy(r => r.IsVisible);

            return query.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                BookingId = r.BookingId,
                CourtId = r.CourtId,
                UserFullName = r.User.FullName,
                UserAvatarUrl = r.User.AvatarUrl,
                Rating = r.Rating,
                Comment = r.Comment,
            });
        }
    }
}
