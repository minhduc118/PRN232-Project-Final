using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>System access role (Admin, Staff, Coach, Customer, Manager).</summary>
public class Role
{
  /// <summary>Primary key.</summary>
  public int RoleId { get; set; }

  /// <summary>Unique role name.</summary>
  [Required, MaxLength(50)]
  public string RoleName { get; set; } = string.Empty;

  /// <summary>Human-readable role description.</summary>
  [MaxLength(200)]
  public string? Description { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
