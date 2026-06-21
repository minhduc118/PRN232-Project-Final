using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs.Courts;
using SportCourtManagerment.DTOs.CourtTypes;
using SportCourtManagerment.DTOs.Home;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Services.Implementations;

/// <summary>
/// Home page business logic: aggregates court types, featured courts,
/// active promotions, and statistics into a single response.
/// </summary>
public class HomeService : IHomeService
{
  private readonly ApplicationDbContext _db;
  private readonly ICourtService        _courtService;
  private readonly IPromotionService    _promotionService;

  public HomeService(
    ApplicationDbContext db,
    ICourtService        courtService,
    IPromotionService    promotionService)
  {
    _db               = db;
    _courtService     = courtService;
    _promotionService = promotionService;
  }

  /// <inheritdoc/>
  public async Task<HomeDataDto> GetHomeDataAsync()
  {
    // 1. Court types with active court counts
    var courtTypes = await _db.CourtTypes
      .AsNoTracking()
      .Where(ct => ct.IsActive)
      .Select(ct => new CourtTypeDto
      {
        CourtTypeId = ct.CourtTypeId,
        TypeName    = ct.TypeName,
        IconUrl     = ct.IconUrl,
        Description = ct.Description,
        CourtCount  = ct.Courts.Count(c => c.Status != CourtStatus.Inactive),
      })
      .OrderBy(ct => ct.TypeName)
      .ToListAsync();

    // 2. Top 6 courts by average rating
    var topRated = await _courtService.GetCourtsODataQueryable()
      .Where(c => c.AverageRating != null)
      .OrderByDescending(c => c.AverageRating)
      .ThenByDescending(c => c.ReviewCount)
      .Take(6)
      .ToListAsync();

    // 3. Active promotions
    var activePromotions = await _promotionService.GetActivePromotionsAsync();

    // 4. Statistics
    var totalCourts = await _db.Courts
      .CountAsync(c => c.Status != CourtStatus.Inactive);

    var totalBookings = await _db.Bookings
      .CountAsync(b => b.Status == BookingStatus.Completed);

    return new HomeDataDto
    {
      CourtTypes       = courtTypes,
      TopRatedCourts   = topRated,
      ActivePromotions = activePromotions,
      TotalCourts      = totalCourts,
      TotalBookings    = totalBookings,
    };
  }
}
