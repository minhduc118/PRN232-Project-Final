using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.DTOs.Review;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                _ => projected.OrderBy(c => c.CourtName),

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

            // Get all bookings for this court on the given date (non-cancelled and non-expired)
            var targetDate = date.Date;
            var now = DateTime.UtcNow;
            var bookedSlotIds = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.CourtId == courtId
                             && b.BookingDate.Date == targetDate
                             && b.Status != BookingStatus.Cancelled
                             && (b.Status != BookingStatus.Pending || !b.ExpiredAt.HasValue || b.ExpiredAt > now))
                .Select(b => b.SlotId)
                .ToListAsync();

            // Check if court is under maintenance on this date
            var isUnderMaintenance = court.Status == CourtStatus.Maintenance;

            var slots = court.CourtPricings
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

        // --- Admin CRUD methods from feature/model-admin ---

        public async Task<IEnumerable<CourtDto>> GetAllAsync(int? complexId, string? status)
        {
            var courts = await _courtRepo.GetAllWithDetailsAsync(complexId, status);
            return courts.Select(MapToDto).ToList();
        }

        public async Task<CourtDto?> GetByIdAsync(int id)
        {
            var court = await _courtRepo.GetByIdWithDetailsAsync(id);
            return court == null ? null : MapToDto(court);
        }

        public async Task<CourtDto> CreateAsync(CourtDto dto)
        {
            if (!System.TimeSpan.TryParse(dto.OpenTime, out var openTime) ||
                !System.TimeSpan.TryParse(dto.CloseTime, out var closeTime))
            {
                throw new System.ArgumentException("Giờ mở/đóng cửa không đúng định dạng.");
            }

            if (!System.Enum.TryParse<CourtStatus>(dto.Status, true, out var status))
            {
                status = CourtStatus.Available;
            }

            var court = new Court
            {
                CourtName = dto.CourtName,
                CourtCode = dto.CourtCode,
                CourtTypeId = dto.CourtTypeId,
                ComplexId = dto.ComplexId,
                Status = status,
                OpenTime = openTime,
                CloseTime = closeTime,
                PricePerHour = dto.PricePerHour,
                CourtSize = dto.CourtSize,
                IsDeleted = false
            };

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                court.CourtImages.Add(new CourtImage
                {
                    ImageUrl = dto.ImageUrl,
                    IsPrimary = true
                });
            }

            await _courtRepo.AddAsync(court);

            var loaded = await _courtRepo.GetByIdWithDetailsAsync(court.CourtId);
            return loaded == null ? dto : MapToDto(loaded);
        }

        public async Task UpdateAsync(int id, CourtDto dto)
        {
            var court = await _courtRepo.GetByIdWithDetailsAsync(id);
            if (court == null) throw new System.Collections.Generic.KeyNotFoundException("Không tìm thấy sân.");

            if (!System.TimeSpan.TryParse(dto.OpenTime, out var openTime) ||
                !System.TimeSpan.TryParse(dto.CloseTime, out var closeTime))
            {
                throw new System.ArgumentException("Giờ mở/đóng cửa không đúng định dạng.");
            }

            if (!System.Enum.TryParse<CourtStatus>(dto.Status, true, out var status))
            {
                status = CourtStatus.Available;
            }

            court.CourtName = dto.CourtName;
            court.CourtCode = dto.CourtCode;
            court.CourtTypeId = dto.CourtTypeId;
            court.ComplexId = dto.ComplexId;
            court.Status = status;
            court.OpenTime = openTime;
            court.CloseTime = closeTime;
            court.PricePerHour = dto.PricePerHour;
            court.CourtSize = dto.CourtSize;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                var primary = court.CourtImages.FirstOrDefault(i => i.IsPrimary);
                if (primary != null)
                {
                    primary.ImageUrl = dto.ImageUrl;
                }
                else
                {
                    court.CourtImages.Add(new CourtImage
                    {
                        ImageUrl = dto.ImageUrl,
                        IsPrimary = true
                    });
                }
            }

            await _courtRepo.UpdateAsync(court);
        }

        public async Task DeleteAsync(int id)
        {
            await _courtRepo.SoftDeleteAsync(id);
        }

        public async Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null)
        {
            return await _courtRepo.ExistsByCodeAsync(courtCode, excludeCourtId);
        }

        private static CourtDto MapToDto(Court c) => new()
        {
            CourtId = c.CourtId,
            CourtName = c.CourtName,
            CourtCode = c.CourtCode,
            CourtTypeId = c.CourtTypeId,
            CourtTypeName = c.CourtType.TypeName,
            ComplexId = c.ComplexId,
            ComplexName = c.Complex.ComplexName,
            Status = c.Status.ToString(),
            OpenTime = c.OpenTime.ToString(@"hh\:mm"),
            CloseTime = c.CloseTime.ToString(@"hh\:mm"),
            PricePerHour = c.PricePerHour,
            CourtSize = c.CourtSize,
            ImageUrl = c.CourtImages.OrderBy(i => i.CourtImageId).Select(i => i.ImageUrl).FirstOrDefault()
        };
    }
}
