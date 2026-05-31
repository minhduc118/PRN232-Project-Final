using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Court reservation made by a customer for a specific date and time slot.</summary>
public class Booking
{
  /// <summary>Primary key.</summary>
  public int BookingId { get; set; }

  /// <summary>Human-readable unique booking reference code.</summary>
  [Required, MaxLength(20)]
  public string BookingCode { get; set; } = string.Empty;

  /// <summary>FK to the customer who made this booking.</summary>
  public int UserId { get; set; }

  /// <summary>FK to the booked court.</summary>
  public int CourtId { get; set; }

  /// <summary>FK to the reserved time slot.</summary>
  public int SlotId { get; set; }

  /// <summary>FK to recurring schedule if part of one (nullable).</summary>
  public int? RecurringId { get; set; }

  /// <summary>Calendar date of the booking.</summary>
  public DateOnly BookingDate { get; set; }

  /// <summary>Actual start time (may differ from slot if flexible).</summary>
  public TimeOnly StartTime { get; set; }

  /// <summary>Actual end time.</summary>
  public TimeOnly EndTime { get; set; }

  /// <summary>Pre-discount subtotal in VND.</summary>
  public decimal SubTotal { get; set; }

  /// <summary>Amount discounted from promo or membership tier.</summary>
  public decimal DiscountAmount { get; set; }

  /// <summary>Final amount the customer must pay.</summary>
  public decimal TotalAmount { get; set; }

  /// <summary>FK to applied promotion (nullable).</summary>
  public int? PromotionId { get; set; }

  /// <summary>Current booking lifecycle status.</summary>
  public BookingStatus Status { get; set; }

  /// <summary>Reason provided when booking is cancelled.</summary>
  [MaxLength(500)]
  public string? CancelReason { get; set; }

  /// <summary>Timestamp when booking was cancelled.</summary>
  public DateTime? CancelledAt { get; set; }

  /// <summary>Additional customer note for the booking.</summary>
  [MaxLength(500)]
  public string? Note { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>Last update timestamp.</summary>
  public DateTime? UpdatedAt { get; set; }

  // Navigation properties
  public User User { get; set; } = null!;
  public Court Court { get; set; } = null!;
  public TimeSlot TimeSlot { get; set; } = null!;
  public RecurringBooking? RecurringBooking { get; set; }
  public Promotion? Promotion { get; set; }
  public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
  public ICollection<Payment> Payments { get; set; } = new List<Payment>();
  public Review? Review { get; set; }
  public Invoice? Invoice { get; set; }
  public PlayerRequest? PlayerRequest { get; set; }
}
