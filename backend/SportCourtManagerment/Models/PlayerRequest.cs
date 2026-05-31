using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Player matchmaking post seeking teammates for an existing booking.</summary>
public class PlayerRequest
{
  /// <summary>Primary key.</summary>
  public int RequestId { get; set; }

  /// <summary>FK to the booking this request is linked to.</summary>
  public int BookingId { get; set; }

  /// <summary>FK to user who created the matchmaking post.</summary>
  public int HostUserId { get; set; }

  /// <summary>Required skill level for joining players.</summary>
  public SkillLevel SkillLevel { get; set; }

  /// <summary>Number of additional players needed.</summary>
  public int RequiredPlayers { get; set; }

  /// <summary>Gender preference for joining players (null = any).</summary>
  public Gender? GenderPref { get; set; }

  /// <summary>Minimum age for joining players (optional).</summary>
  public int? AgeMin { get; set; }

  /// <summary>Maximum age for joining players (optional).</summary>
  public int? AgeMax { get; set; }

  /// <summary>Additional notes for prospective players.</summary>
  [MaxLength(500)]
  public string? Description { get; set; }

  /// <summary>Current status of the matchmaking post.</summary>
  public PlayerRequestStatus Status { get; set; }

  /// <summary>When this request automatically closes if unfilled.</summary>
  public DateTime? ExpiresAt { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public Booking Booking { get; set; } = null!;
  public User HostUser { get; set; } = null!;
  public ICollection<PlayerRequestMember> Members { get; set; } = new List<PlayerRequestMember>();
}
