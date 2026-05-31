using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Bookable time slot defining a period within a day.</summary>
public class TimeSlot
{
  /// <summary>Primary key.</summary>
  public int SlotId { get; set; }

  /// <summary>Human-readable slot label (e.g. Giờ vàng).</summary>
  [Required, MaxLength(50)]
  public string SlotName { get; set; } = string.Empty;

  /// <summary>Slot start time of day.</summary>
  public TimeOnly StartTime { get; set; }

  /// <summary>Slot end time of day.</summary>
  public TimeOnly EndTime { get; set; }

  /// <summary>Day classification affecting pricing.</summary>
  public DayType DayType { get; set; }

  /// <summary>Whether this slot is available for booking.</summary>
  public bool IsActive { get; set; }

  // Navigation properties
  public ICollection<CourtPricing> CourtPricings { get; set; } = new List<CourtPricing>();
  public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
  public ICollection<RecurringBooking> RecurringBookings { get; set; } = new List<RecurringBooking>();
  public ICollection<CoachSchedule> CoachSchedules { get; set; } = new List<CoachSchedule>();
  public ICollection<Waitlist> Waitlists { get; set; } = new List<Waitlist>();
}
