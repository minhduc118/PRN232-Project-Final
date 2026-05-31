using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Coach availability session available for student bookings.</summary>
public class CoachSchedule
{
  /// <summary>Primary key.</summary>
  public int ScheduleId { get; set; }

  /// <summary>FK to the coach (user with Coach role).</summary>
  public int CoachId { get; set; }

  /// <summary>FK to court where coaching session is held.</summary>
  public int CourtId { get; set; }

  /// <summary>FK to time slot for the session.</summary>
  public int SlotId { get; set; }

  /// <summary>Date of the coaching session.</summary>
  public DateOnly ScheduleDate { get; set; }

  /// <summary>Maximum number of students for this session.</summary>
  public int MaxStudents { get; set; }

  /// <summary>Session price per student in VND.</summary>
  public decimal Price { get; set; }

  /// <summary>Coach notes for students.</summary>
  [MaxLength(300)]
  public string? Note { get; set; }

  /// <summary>Whether the session is fully booked.</summary>
  public bool IsBooked { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public User Coach { get; set; } = null!;
  public Court Court { get; set; } = null!;
  public TimeSlot TimeSlot { get; set; } = null!;
}
