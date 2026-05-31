using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Sport court category (Badminton, Football, Tennis, etc.).</summary>
public class CourtType
{
  /// <summary>Primary key.</summary>
  public int CourtTypeId { get; set; }

  /// <summary>Unique type name (e.g. Cầu lông).</summary>
  [Required, MaxLength(100)]
  public string TypeName { get; set; } = string.Empty;

  /// <summary>URL to sport type icon image.</summary>
  [MaxLength(500)]
  public string? IconUrl { get; set; }

  /// <summary>Description of the court type.</summary>
  [MaxLength(300)]
  public string? Description { get; set; }

  /// <summary>Whether this type is active and bookable.</summary>
  public bool IsActive { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public ICollection<Court> Courts { get; set; } = new List<Court>();
}
