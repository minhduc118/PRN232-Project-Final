using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Staff work shift assignment with actual check-in/out tracking.</summary>
public class StaffShift
{
  /// <summary>Primary key.</summary>
  public int ShiftId { get; set; }

  /// <summary>FK to staff member assigned to this shift.</summary>
  public int StaffId { get; set; }

  /// <summary>Date on which the shift occurs.</summary>
  public DateOnly ShiftDate { get; set; }

  /// <summary>Shift period classification.</summary>
  public ShiftType ShiftType { get; set; }

  /// <summary>Scheduled shift start time.</summary>
  public TimeOnly StartTime { get; set; }

  /// <summary>Scheduled shift end time.</summary>
  public TimeOnly EndTime { get; set; }

  /// <summary>Actual check-in timestamp (nullable until staff arrives).</summary>
  public DateTime? CheckInTime { get; set; }

  /// <summary>Actual check-out timestamp (nullable until shift ends).</summary>
  public DateTime? CheckOutTime { get; set; }

  /// <summary>Shift notes or attendance remarks.</summary>
  [MaxLength(300)]
  public string? Note { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public User Staff { get; set; } = null!;
}
