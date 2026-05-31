using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Sports court with location, availability and pricing info.</summary>
public class Court
{
  /// <summary>Primary key.</summary>
  public int CourtId { get; set; }

  /// <summary>Display name for the court.</summary>
  [Required, MaxLength(100)]
  public string CourtName { get; set; } = string.Empty;

  /// <summary>Unique internal court code (e.g. CL-A1).</summary>
  [Required, MaxLength(20)]
  public string CourtCode { get; set; } = string.Empty;

  /// <summary>FK to sport type category.</summary>
  public int CourtTypeId { get; set; }

  /// <summary>Detailed description shown on booking page.</summary>
  [MaxLength(1000)]
  public string? Description { get; set; }

  /// <summary>Physical location within the facility.</summary>
  [MaxLength(300)]
  public string? Location { get; set; }

  /// <summary>Maximum number of players.</summary>
  public int? Capacity { get; set; }

  /// <summary>Surface material (e.g. Gỗ, Nhựa PVC, Cỏ nhân tạo).</summary>
  [MaxLength(100)]
  public string? Surface { get; set; }

  /// <summary>Primary image URL for court listing.</summary>
  [MaxLength(500)]
  public string? ImageUrl { get; set; }

  /// <summary>Daily opening time.</summary>
  public TimeOnly OpenTime { get; set; }

  /// <summary>Daily closing time.</summary>
  public TimeOnly CloseTime { get; set; }

  /// <summary>Current availability status.</summary>
  public CourtStatus Status { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>Last update timestamp.</summary>
  public DateTime? UpdatedAt { get; set; }

  // Navigation properties
  public CourtType CourtType { get; set; } = null!;
  public ICollection<CourtImage> CourtImages { get; set; } = new List<CourtImage>();
  public ICollection<CourtPricing> CourtPricings { get; set; } = new List<CourtPricing>();
  public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
  public ICollection<Review> Reviews { get; set; } = new List<Review>();
  public ICollection<RecurringBooking> RecurringBookings { get; set; } = new List<RecurringBooking>();
  public ICollection<CoachSchedule> CoachSchedules { get; set; } = new List<CoachSchedule>();
  public ICollection<Waitlist> Waitlists { get; set; } = new List<Waitlist>();
  public ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();
}
