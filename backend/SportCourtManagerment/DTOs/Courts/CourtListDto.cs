namespace SportCourtManagerment.DTOs.Courts;

/// <summary>
/// DTO for court listing / search results.
/// Contains summary info with pricing range and rating.
/// </summary>
public class CourtListDto
{
  public int CourtId { get; set; }
  public string CourtName { get; set; } = string.Empty;
  public string CourtCode { get; set; } = string.Empty;
  public string CourtTypeName { get; set; } = string.Empty;
  public int CourtTypeId { get; set; }
  public string? Location { get; set; }
  public string? ImageUrl { get; set; }
  public string? Surface { get; set; }
  public int? Capacity { get; set; }
  public string Status { get; set; } = string.Empty;
  public TimeOnly OpenTime { get; set; }
  public TimeOnly CloseTime { get; set; }

  /// <summary>Lowest price among all court pricings.</summary>
  public decimal? MinPrice { get; set; }

  /// <summary>Highest price among all court pricings.</summary>
  public decimal? MaxPrice { get; set; }

  /// <summary>Average star rating (1–5), null if no reviews.</summary>
  public double? AverageRating { get; set; }

  /// <summary>Total number of visible reviews.</summary>
  public int ReviewCount { get; set; }
}
