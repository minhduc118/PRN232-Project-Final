using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Courts;
using SportCourtManagerment.DTOs.CourtTypes;
using SportCourtManagerment.DTOs.Reviews;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Repositories.Interfaces;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Services.Implementations;

/// <summary>
/// Court business logic: search with filters, detail mapping,
/// and time-slot availability checking.
/// </summary>
public class CourtService : ICourtService
{
  private readonly ICourtRepository  _courtRepo;
  private readonly IReviewRepository _reviewRepo;
  private readonly ApplicationDbContext _db;

  public CourtService(
    ICourtRepository  courtRepo,
    IReviewRepository reviewRepo,
    ApplicationDbContext db)
  {
    _courtRepo  = courtRepo;
    _reviewRepo = reviewRepo;
    _db         = db;
  }

  /// <inheritdoc/>
  public async Task<PagedResult<CourtListDto>> SearchCourtsAsync(CourtSearchParams p)
  {
    var query = _courtRepo.GetCourtsQueryable();

    // ── Filters ──────────────────────────────────────────
    if (p.CourtTypeId.HasValue)
      query = query.Where(c => c.CourtTypeId == p.CourtTypeId.Value);

    if (!string.IsNullOrWhiteSpace(p.Status)
        && Enum.TryParse<CourtStatus>(p.Status, true, out var status))
      query = query.Where(c => c.Status == status);

    if (!string.IsNullOrWhiteSpace(p.SearchTerm))
    {
      var term = p.SearchTerm.Trim().ToLower();
      query = query.Where(c =>
        c.CourtName.ToLower().Contains(term) ||
        (c.Location != null && c.Location.ToLower().Contains(term)));
    }

    if (p.MinPrice.HasValue)
      query = query.Where(c => c.CourtPricings.Any(cp => cp.Price >= p.MinPrice.Value));

    if (p.MaxPrice.HasValue)
      query = query.Where(c => c.CourtPricings.Any(cp => cp.Price <= p.MaxPrice.Value));

    // Filter by availability on a specific date + time slot
    if (p.Date.HasValue)
    {
      var date = p.Date.Value;
      query = query.Where(c =>
        c.Status == CourtStatus.Available &&
        // Exclude courts that have a booking for this date/slot
        !c.Bookings.Any(b =>
          b.BookingDate == date &&
          b.Status != BookingStatus.Cancelled &&
          (!p.TimeSlotId.HasValue || b.SlotId == p.TimeSlotId.Value)));
    }

    // ── Projection (before sorting to enable sort by computed fields) ──
    var projected = query.Select(c => new CourtListDto
    {
      CourtId       = c.CourtId,
      CourtName     = c.CourtName,
      CourtCode     = c.CourtCode,
      CourtTypeName = c.CourtType.TypeName,
      CourtTypeId   = c.CourtTypeId,
      Location      = c.Location,
      ImageUrl      = c.ImageUrl,
      Surface       = c.Surface,
      Capacity      = c.Capacity,
      Status        = c.Status.ToString(),
      OpenTime      = c.OpenTime,
      CloseTime     = c.CloseTime,
      MinPrice      = c.CourtPricings.Any() ? c.CourtPricings.Min(cp => cp.Price) : null,
      MaxPrice      = c.CourtPricings.Any() ? c.CourtPricings.Max(cp => cp.Price) : null,
      AverageRating = c.Reviews.Any(r => r.IsVisible)
                        ? c.Reviews.Where(r => r.IsVisible).Average(r => (double)r.Rating)
                        : null,
      ReviewCount   = c.Reviews.Count(r => r.IsVisible),
    });

    // ── Sorting ──────────────────────────────────────────
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

    // ── Pagination ───────────────────────────────────────
    var totalCount = await projected.CountAsync();
    var items = await projected
      .Skip((p.PageNumber - 1) * p.PageSize)
      .Take(p.PageSize)
      .ToListAsync();

    return new PagedResult<CourtListDto>
    {
      Items      = items,
      TotalCount = totalCount,
      PageNumber = p.PageNumber,
      PageSize   = p.PageSize,
    };
  }

  /// <inheritdoc/>
  public async Task<CourtDetailDto?> GetCourtDetailAsync(int courtId)
  {
    var court = await _courtRepo.GetCourtDetailAsync(courtId);
    if (court is null) return null;

    // Get full rating summary
    var (avgRating, totalCount, distribution) =
      await _reviewRepo.GetCourtRatingSummaryAsync(courtId);

    return new CourtDetailDto
    {
      CourtId     = court.CourtId,
      CourtName   = court.CourtName,
      CourtCode   = court.CourtCode,
      Description = court.Description,
      Location    = court.Location,
      Surface     = court.Surface,
      Capacity    = court.Capacity,
      ImageUrl    = court.ImageUrl,
      OpenTime    = court.OpenTime,
      CloseTime   = court.CloseTime,
      Status      = court.Status.ToString(),
      CreatedAt   = court.CreatedAt,
      CourtType = new CourtTypeDto
      {
        CourtTypeId = court.CourtType.CourtTypeId,
        TypeName    = court.CourtType.TypeName,
        IconUrl     = court.CourtType.IconUrl,
        Description = court.CourtType.Description,
        CourtCount  = 0, // not needed in detail view
      },
      Images = court.CourtImages.Select(ci => new CourtImageDto
      {
        ImageId   = ci.ImageId,
        ImageUrl  = ci.ImageUrl,
        IsPrimary = ci.IsPrimary,
        SortOrder = ci.SortOrder,
      }).ToList(),
      Pricings = court.CourtPricings.Select(cp => new CourtPricingDto
      {
        PricingId      = cp.PricingId,
        SlotId         = cp.SlotId,
        SlotName       = cp.TimeSlot.SlotName,
        StartTime      = cp.TimeSlot.StartTime,
        EndTime        = cp.TimeSlot.EndTime,
        DayType        = cp.TimeSlot.DayType.ToString(),
        Price          = cp.Price,
        PeakMultiplier = cp.PeakMultiplier,
      }).OrderBy(p => p.StartTime).ToList(),
      ReviewSummary = new CourtReviewSummaryDto
      {
        AverageRating      = avgRating,
        TotalReviews       = totalCount,
        RatingDistribution = distribution,
      },
    };
  }

  /// <inheritdoc/>
  public async Task<CourtAvailabilityDto?> GetCourtAvailabilityAsync(int courtId, DateOnly date)
  {
    var court = await _courtRepo.GetCourtWithPricingsAsync(courtId);
    if (court is null) return null;

    // Get all bookings for this court on the given date (non-cancelled)
    var bookedSlotIds = await _db.Bookings
      .AsNoTracking()
      .Where(b => b.CourtId == courtId
                   && b.BookingDate == date
                   && b.Status != BookingStatus.Cancelled)
      .Select(b => b.SlotId)
      .ToListAsync();

    // Check if court is under maintenance on this date
    var isUnderMaintenance = court.Status == CourtStatus.Maintenance;

    var slots = court.CourtPricings
      .Where(cp => cp.TimeSlot.IsActive)
      .Select(cp => new AvailabilitySlotDto
      {
        SlotId    = cp.SlotId,
        SlotName  = cp.TimeSlot.SlotName,
        StartTime = cp.TimeSlot.StartTime,
        EndTime   = cp.TimeSlot.EndTime,
        Price     = cp.Price * cp.PeakMultiplier,
        Status    = isUnderMaintenance
                      ? "Maintenance"
                      : bookedSlotIds.Contains(cp.SlotId)
                        ? "Booked"
                        : "Available",
      })
      .OrderBy(s => s.StartTime)
      .ToList();

    return new CourtAvailabilityDto
    {
      CourtId   = court.CourtId,
      CourtName = court.CourtName,
      Date      = date,
      Slots     = slots,
    };
  }

  /// <inheritdoc/>
  public IQueryable<CourtListDto> GetCourtsODataQueryable()
  {
    return _courtRepo.GetCourtsQueryable()
      .Select(c => new CourtListDto
      {
        CourtId       = c.CourtId,
        CourtName     = c.CourtName,
        CourtCode     = c.CourtCode,
        CourtTypeName = c.CourtType.TypeName,
        CourtTypeId   = c.CourtTypeId,
        Location      = c.Location,
        ImageUrl      = c.ImageUrl,
        Surface       = c.Surface,
        Capacity      = c.Capacity,
        Status        = c.Status.ToString(),
        OpenTime      = c.OpenTime,
        CloseTime     = c.CloseTime,
        MinPrice      = c.CourtPricings.Any() ? c.CourtPricings.Min(cp => cp.Price) : null,
        MaxPrice      = c.CourtPricings.Any() ? c.CourtPricings.Max(cp => cp.Price) : null,
        AverageRating = c.Reviews.Any(r => r.IsVisible)
                          ? c.Reviews.Where(r => r.IsVisible).Average(r => (double)r.Rating)
                          : null,
        ReviewCount   = c.Reviews.Count(r => r.IsVisible),
      });
  }
}
