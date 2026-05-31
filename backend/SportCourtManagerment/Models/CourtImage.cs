using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Photo gallery image attached to a court listing.</summary>
public class CourtImage
{
  /// <summary>Primary key.</summary>
  public int ImageId { get; set; }

  /// <summary>FK to owning court.</summary>
  public int CourtId { get; set; }

  /// <summary>Absolute URL to the image file.</summary>
  [Required, MaxLength(500)]
  public string ImageUrl { get; set; } = string.Empty;

  /// <summary>Whether this image is the primary display thumbnail.</summary>
  public bool IsPrimary { get; set; }

  /// <summary>Display order index (ascending).</summary>
  public int SortOrder { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public Court Court { get; set; } = null!;
}
