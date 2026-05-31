using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Audit trail record for sensitive data changes in the system.</summary>
public class AuditLog
{
  /// <summary>Primary key.</summary>
  public int LogId { get; set; }

  /// <summary>FK to user who performed the action (nullable for system actions).</summary>
  public int? UserId { get; set; }

  /// <summary>Action performed (e.g. Create, Update, Delete, Login).</summary>
  [Required, MaxLength(100)]
  public string Action { get; set; } = string.Empty;

  /// <summary>Database table that was affected.</summary>
  [Required, MaxLength(100)]
  public string TableName { get; set; } = string.Empty;

  /// <summary>Primary key of the affected record.</summary>
  public int? RecordId { get; set; }

  /// <summary>JSON snapshot of values before the change.</summary>
  public string? OldValues { get; set; }

  /// <summary>JSON snapshot of values after the change.</summary>
  public string? NewValues { get; set; }

  /// <summary>IP address of the requesting client.</summary>
  [MaxLength(50)]
  public string? IpAddress { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public User? User { get; set; }
}
