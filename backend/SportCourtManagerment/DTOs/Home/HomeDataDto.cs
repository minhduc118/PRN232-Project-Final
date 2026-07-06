using SportCourtManagerment.DTOs.Courts;
using SportCourtManagerment.DTOs.CourtTypes;
using SportCourtManagerment.DTOs.Promotions;

namespace SportCourtManagerment.DTOs.Home;

/// <summary>
/// Aggregated data bundle for the customer landing page (GET /api/home).
/// Provides court types, top-rated courts, active promotions, and statistics.
/// </summary>
public class HomeDataDto
{
  /// <summary>Active sport categories with court counts for the icon grid.</summary>
  public List<SportCourtManagerment.DTOs.CourtTypes.CourtTypeDto> CourtTypes { get; set; } = new();

  /// <summary>Top-rated courts for the featured section (max 6).</summary>
  public List<CourtListDto> TopRatedCourts { get; set; } = new();

  /// <summary>Currently active promotion banners.</summary>
  public List<PromotionDto> ActivePromotions { get; set; } = new();

  /// <summary>Total number of available courts in the system.</summary>
  public int TotalCourts { get; set; }

  /// <summary>Total completed bookings (social proof).</summary>
  public int TotalBookings { get; set; }
}
