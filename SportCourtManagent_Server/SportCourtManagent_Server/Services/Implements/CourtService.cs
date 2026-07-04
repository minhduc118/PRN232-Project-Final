using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.DTOs.Review;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    
    public class CourtService : ICourtService
    {
        private readonly ICourtRepository _courtRepo;
        private readonly IReviewRepository _reviewRepo;
        private readonly AppDbContext _db;

        public CourtService(
            ICourtRepository courtRepo,
            IReviewRepository reviewRepo,
            AppDbContext db)
        {
            _courtRepo = courtRepo;
            _reviewRepo = reviewRepo;
            _db = db;
        }


        public async Task<PagedResult<CourtListDto>> SearchCourtsAsync(CourtSearchParams p)
        {
            var query = _courtRepo.GetCourtsQueryable();

            // Filters
            if (p.CourtTypeId.HasValue)
                query = query.Where(c => c.CourtTypeId == p.CourtTypeId.Value);

            if (!string.IsNullOrWhiteSpace(p.Status)
                && Enum.TryParse<CourtStatus>(p.Status, true, out var status))
                query = query.Where(c => c.Status == status);

            if (!string.IsNullOrWhiteSpace(p.SearchTerm))
            {
                var term = p.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.CourtName.ToLower().Contains(term));
            }

            if (p.MinPrice.HasValue)
                query = query.Where(c => c.CourtPricings.Any(cp => cp.Price >= p.MinPrice.Value));

            if (p.MaxPrice.HasValue)
                query = query.Where(c => c.CourtPricings.Any(cp => cp.Price <= p.MaxPrice.Value));

            if (p.TimeSlotId.HasValue)
            {
                query = query.Where(c => c.CourtPricings.Any(cp => cp.SlotId == p.TimeSlotId.Value));
            }

            // Filter by availability on a specific date + time slot
            if (p.Date.HasValue)
            {
                var date = p.Date.Value.Date;
                query = query.Where(c =>
                    c.Status == CourtStatus.Available &&
                    // Exclude courts that have a booking for this date/slot
                    !c.Bookings.Any(b =>
                        b.BookingDate.Date == date &&
                        b.Status != BookingStatus.Cancelled &&
                        (!p.TimeSlotId.HasValue || b.SlotId == p.TimeSlotId.Value)));
            }

            
            var projected = query.Select(c => new CourtListDto
            {
                CourtId = c.CourtId,
                CourtName = c.CourtName,
                CourtCode = c.CourtCode,
                CourtTypeName = c.CourtType.TypeName,
                CourtTypeId = c.CourtTypeId,
                CourtSize = c.CourtSize,
                Location = c.Complex.ComplexName + " - " + c.Complex.Address,
                ImageUrl = c.CourtImages.OrderByDescending(ci => ci.IsPrimary).Select(ci => ci.ImageUrl).FirstOrDefault(),
                Status = c.Status.ToString(),
                OpenTime = c.OpenTime,
                CloseTime = c.CloseTime,
                PricePerHour = c.PricePerHour,
                MinPrice = c.CourtPricings.Any() ? c.CourtPricings.Min(cp => cp.Price) : null,
                MaxPrice = c.CourtPricings.Any() ? c.CourtPricings.Max(cp => cp.Price) : null,
                AverageRating = c.Reviews.Any(r => r.IsVisible)
                                    ? c.Reviews.Where(r => r.IsVisible).Average(r => (double)r.Rating)
                                    : null,
                ReviewCount = c.Reviews.Count(r => r.IsVisible),
            });

            //Sorting
            projected = (p.SortBy?.ToLower()) switch
            {
                "price" => p.SortDescending
                    ? projected.OrderByDescending(c => c.MinPrice)
                    : projected.OrderBy(c => c.MinPrice),
                "rating" => p.SortDescending
                    ? projected.OrderByDescending(c => c.AverageRating)
                    : projected.OrderBy(c => c.AverageRating),
                "name" => p.SortDescending
                    ? projected.OrderByDescending(c => c.CourtName)
                    : projected.OrderBy(c => c.CourtName),
                _ => projected.OrderBy(c => c.CourtId),
            };

            //Pagination
            var totalCount = await projected.CountAsync();
            var items = await projected
                .Skip((p.PageNumber - 1) * p.PageSize)
                .Take(p.PageSize)
                .ToListAsync();

            return new PagedResult<CourtListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize,
            };
        }

       
        public async Task<CourtDetailDto?> GetCourtDetailAsync(int courtId)
        {
            var court = await _courtRepo.GetCourtDetailAsync(courtId);
            if (court is null) return null;

            // Get full rating summary
            var (avgRating, totalCount, distribution) =
                await _reviewRepo.GetCourtRatingSummaryAsync(courtId);

            return new CourtDetailDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                CourtCode = court.CourtCode,
                CourtSize = court.CourtSize,
                Location = court.Complex.ComplexName + " - " + court.Complex.Address,
                ImageUrl = court.CourtImages.OrderByDescending(ci => ci.IsPrimary).Select(ci => ci.ImageUrl).FirstOrDefault(),
                OpenTime = court.OpenTime,
                CloseTime = court.CloseTime,
                PricePerHour = court.PricePerHour,
                Status = court.Status.ToString(),
                CourtType = new CourtTypeDto
                {
                    CourtTypeId = court.CourtType.CourtTypeId,
                    TypeName = court.CourtType.TypeName,
                    IsActive = court.CourtType.IsActive,
                    CourtCount = 0, // not needed in detail view
                },
                Images = court.CourtImages.Select(ci => new CourtImageDto
                {
                    CourtImageId = ci.CourtImageId,
                    ImageUrl = ci.ImageUrl,
                    IsPrimary = ci.IsPrimary,
                }).ToList(),
                Pricings = court.CourtPricings.Select(cp => new CourtPricingDto
                {
                    PricingId = cp.PricingId,
                    SlotId = cp.SlotId,
                    SlotName = cp.TimeSlot?.SlotName ?? "Không xác định",
                    StartTime = cp.TimeSlot?.StartTime ?? TimeSpan.Zero,
                    EndTime = cp.TimeSlot?.EndTime ?? TimeSpan.Zero,
                    DayType = cp.TimeSlot?.DayType.ToString() ?? "Weekday",
                    Price = cp.Price,
                }).OrderBy(p => p.StartTime).ToList(),
                ReviewSummary = new CourtReviewSummaryDto
                {
                    AverageRating = avgRating,
                    TotalReviews = totalCount,
                    RatingDistribution = distribution,
                },
            };
        }

        
        public async Task<CourtAvailabilityDto?> GetCourtAvailabilityAsync(int courtId, DateTime date)
        {
            var court = await _courtRepo.GetCourtWithPricingsAsync(courtId);
            if (court is null) return null;

            // Get all bookings for this court on the given date (non-cancelled)
            var targetDate = date.Date;
            var bookedSlotIds = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.CourtId == courtId
                             && b.BookingDate.Date == targetDate
                             && b.Status != BookingStatus.Cancelled)
                .Select(b => b.SlotId)
                .ToListAsync();

            // Check if court is under maintenance on this date
            var isUnderMaintenance = court.Status == CourtStatus.Maintenance;

            var slots = court.CourtPricings
                .GroupBy(cp => cp.SlotId)
                .Select(g => g.First())
                .Select(cp => new AvailabilitySlotDto
                {
                    SlotId = cp.SlotId,
                    SlotName = cp.TimeSlot?.SlotName ?? "Không xác định",
                    StartTime = cp.TimeSlot?.StartTime ?? TimeSpan.Zero,
                    EndTime = cp.TimeSlot?.EndTime ?? TimeSpan.Zero,
                    Price = cp.Price,
                    Status = isUnderMaintenance
                                ? "Maintenance"
                                : bookedSlotIds.Contains(cp.SlotId)
                                    ? "Booked"
                                    : "Available",
                })
                .OrderBy(s => s.StartTime)
                .ToList();

            return new CourtAvailabilityDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                Date = date,
                Slots = slots,
            };
        }

      
        public IQueryable<CourtListDto> GetCourtsODataQueryable()
        {
            return _courtRepo.GetCourtsQueryable()
                .Select(c => new CourtListDto
                {
                    CourtId = c.CourtId,
                    CourtName = c.CourtName,
                    CourtCode = c.CourtCode,
                    CourtTypeName = c.CourtType.TypeName,
                    CourtTypeId = c.CourtTypeId,
                    CourtSize = c.CourtSize,
                    Location = c.Complex.ComplexName + " - " + c.Complex.Address,
                    ImageUrl = c.CourtImages.OrderByDescending(ci => ci.IsPrimary).Select(ci => ci.ImageUrl).FirstOrDefault(),
                    Status = c.Status.ToString(),
                    OpenTime = c.OpenTime,
                    CloseTime = c.CloseTime,
                    PricePerHour = c.PricePerHour,
                    MinPrice = c.CourtPricings.Any() ? c.CourtPricings.Min(cp => cp.Price) : null,
                    MaxPrice = c.CourtPricings.Any() ? c.CourtPricings.Max(cp => cp.Price) : null,
                    AverageRating = c.Reviews.Any(r => r.IsVisible)
                                        ? c.Reviews.Where(r => r.IsVisible).Average(r => (double)r.Rating)
                                        : null,
                    ReviewCount = c.Reviews.Count(r => r.IsVisible),
                });
        }
    }
}
