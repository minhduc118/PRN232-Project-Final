using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>In-app notification delivered to a specific user.</summary>
public class Notification
{
  /// <summary>Primary key.</summary>
  public int NotificationId { get; set; }

  /// <summary>FK to the target user.</summary>
  public int UserId { get; set; }

  /// <summary>Short notification title.</summary>
  [Required, MaxLength(200)]
  public string Title { get; set; } = string.Empty;

  /// <summary>Notification body message.</summary>
  [Required, MaxLength(1000)]
  public string Body { get; set; } = string.Empty;

  /// <summary>Notification category for icon/routing.</summary>
  public NotificationType Type { get; set; }

  /// <summary>Optional ID of the related entity (e.g. BookingId, WaitlistId).</summary>
  public int? ReferenceId { get; set; }

  /// <summary>Whether the user has read this notification.</summary>
  public bool IsRead { get; set; }

  /// <summary>Timestamp when user opened/read the notification.</summary>
  public DateTime? ReadAt { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public User User { get; set; } = null!;
}
