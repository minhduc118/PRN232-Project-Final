using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Queue entry for a customer waiting for a specific court slot to become available.</summary>
public class Waitlist
{
  /// <summary>Primary key.</summary>
  public int WaitlistId { get; set; }

  /// <summary>FK to the waiting customer.</summary>
  public int UserId { get; set; }

  /// <summary>FK to the desired court.</summary>
  public int CourtId { get; set; }

  /// <summary>FK to the desired time slot.</summary>
  public int SlotId { get; set; }

  /// <summary>Date for which the customer is waiting.</summary>
  public DateOnly WaitDate { get; set; }

  /// <summary>FIFO queue position (lower = earlier entry).</summary>
  public int Position { get; set; }

  /// <summary>Current waitlist entry status.</summary>
  public WaitlistStatus Status { get; set; }

  /// <summary>When the system sent an availability notification.</summary>
  public DateTime? NotifiedAt { get; set; }

  /// <summary>When the notification offer expires (15 minutes after notified).</summary>
  public DateTime? ExpiredAt { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public User User { get; set; } = null!;
  public Court Court { get; set; } = null!;
  public TimeSlot TimeSlot { get; set; } = null!;
}
