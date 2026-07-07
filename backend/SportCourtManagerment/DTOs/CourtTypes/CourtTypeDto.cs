namespace SportCourtManagerment.DTOs.CourtTypes;

/// <summary>
/// DTO representing a sport court category with active court count.
/// Used in landing page icons and search filter sidebar.
/// </summary>
public class CourtTypeDto
{
  public int CourtTypeId { get; set; }
  public string TypeName { get; set; } = string.Empty;
  public string? IconUrl { get; set; }
  public string? Description { get; set; }

  /// <summary>Number of active courts of this type.</summary>
  public int CourtCount { get; set; }
}
