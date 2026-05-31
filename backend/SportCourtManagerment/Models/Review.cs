using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Customer review and rating submitted after completing a booking.</summary>
public class Review
{
  /// <summary>Primary key.</summary>
  public int ReviewId { get; set; }

  /// <summary>FK to the completed booking being reviewed (unique — 1 review per booking).</summary>
  public int BookingId { get; set; }

  /// <summary>FK to the reviewing customer.</summary>
  public int UserId { get; set; }

  /// <summary>FK to the reviewed court.</summary>
  public int CourtId { get; set; }

  /// <summary>Star rating from 1 to 5.</summary>
  public byte Rating { get; set; }

  /// <summary>Written comment (optional).</summary>
  [MaxLength(1000)]
  public string? Comment { get; set; }

  /// <summary>URL to attached review image (optional).</summary>
  [MaxLength(500)]
  public string? ImageUrl { get; set; }

  /// <summary>Whether the review is publicly displayed.</summary>
  public bool IsVisible { get; set; }

  /// <summary>Admin reply to the review.</summary>
  [MaxLength(500)]
  public string? AdminReply { get; set; }

  /// <summary>When the admin replied.</summary>
  public DateTime? RepliedAt { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public Booking Booking { get; set; } = null!;
  public User User { get; set; } = null!;
  public Court Court { get; set; } = null!;
}
