using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.DTOs.Reviews;

/// <summary>
/// Request body for creating a new court review (POST /api/courts/{courtId}/reviews).
/// Each booking can only have one review (1:1 relationship).
/// </summary>
public class CreateReviewDto
{
  /// <summary>FK to the completed booking being reviewed.</summary>
  [Required]
  public int BookingId { get; set; }

  /// <summary>Star rating from 1 to 5.</summary>
  [Required]
  [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5.")]
  public byte Rating { get; set; }

  /// <summary>Written comment (optional, max 1000 chars).</summary>
  [MaxLength(1000, ErrorMessage = "Comment tối đa 1000 ký tự.")]
  public string? Comment { get; set; }

  /// <summary>URL to attached review image (optional).</summary>
  [MaxLength(500)]
  public string? ImageUrl { get; set; }
}
