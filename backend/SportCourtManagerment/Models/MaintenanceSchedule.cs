using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Planned or emergency court maintenance event.</summary>
public class MaintenanceSchedule
{
  /// <summary>Primary key.</summary>
  public int MaintenanceId { get; set; }

  /// <summary>FK to court being maintained.</summary>
  public int CourtId { get; set; }

  /// <summary>Type of maintenance work being performed.</summary>
  public MaintenanceType MaintenanceType { get; set; }

  /// <summary>Scheduled maintenance start date and time.</summary>
  public DateTime StartDateTime { get; set; }

  /// <summary>Scheduled maintenance end date and time.</summary>
  public DateTime EndDateTime { get; set; }

  /// <summary>FK to staff member assigned to perform maintenance (nullable).</summary>
  public int? AssignedStaffId { get; set; }

  /// <summary>Description of why maintenance is needed.</summary>
  [Required, MaxLength(500)]
  public string Reason { get; set; } = string.Empty;

  /// <summary>Summary of work performed, filled after completion.</summary>
  [MaxLength(500)]
  public string? Result { get; set; }

  /// <summary>Current execution status of the maintenance.</summary>
  public MaintenanceStatus Status { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public Court Court { get; set; } = null!;
  public User? AssignedStaff { get; set; }
}
