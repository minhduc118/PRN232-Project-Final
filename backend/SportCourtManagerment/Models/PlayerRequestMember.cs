using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Join application from a player to a matchmaking request.</summary>
public class PlayerRequestMember
{
  /// <summary>Primary key.</summary>
  public int MemberId { get; set; }

  /// <summary>FK to the matchmaking post.</summary>
  public int RequestId { get; set; }

  /// <summary>FK to the applying player.</summary>
  public int UserId { get; set; }

  /// <summary>Host's decision on this application.</summary>
  public MemberRequestStatus Status { get; set; }

  /// <summary>Timestamp when the player applied.</summary>
  public DateTime JoinedAt { get; set; }

  // Navigation properties
  public PlayerRequest PlayerRequest { get; set; } = null!;
  public User User { get; set; } = null!;
}
