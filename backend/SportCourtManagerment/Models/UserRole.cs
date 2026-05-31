namespace SportCourtManagerment.Models;

/// <summary>Many-to-many mapping between User and Role.</summary>
public class UserRole
{
  /// <summary>Primary key.</summary>
  public int UserRoleId { get; set; }

  /// <summary>FK to assigned user.</summary>
  public int UserId { get; set; }

  /// <summary>FK to assigned role.</summary>
  public int RoleId { get; set; }

  /// <summary>When the role was assigned.</summary>
  public DateTime AssignedAt { get; set; }

  // Navigation properties
  public User User { get; set; } = null!;
  public Role Role { get; set; } = null!;
}
