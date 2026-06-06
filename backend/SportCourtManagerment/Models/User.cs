using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>System user (customer, staff, coach, admin).</summary>
public class User
{
  /// <summary>Primary key.</summary>
  public int UserId { get; set; }

  /// <summary>Full display name in Vietnamese.</summary>
  [Required, MaxLength(100)]
  public string FullName { get; set; } = string.Empty;

  /// <summary>Unique login email address.</summary>
  [Required, MaxLength(100)]
  public string Email { get; set; } = string.Empty;

  /// <summary>Phone number (optional).</summary>
  [MaxLength(15)]
  public string? Phone { get; set; }

  /// <summary>BCrypt hashed password.</summary>
  [Required, MaxLength(255)]
  public string PasswordHash { get; set; } = string.Empty;

  /// <summary>Profile avatar URL.</summary>
  [MaxLength(500)]
  public string? AvatarUrl { get; set; }

  /// <summary>Date of birth for age-related features.</summary>
  public DateOnly? DateOfBirth { get; set; }

  /// <summary>User gender preference.</summary>
  public Gender? Gender { get; set; }

  /// <summary>Accumulated loyalty points for tier upgrade.</summary>
  public int LoyaltyPoints { get; set; }

  /// <summary>FK to current membership tier.</summary>
  public int? MembershipTierId { get; set; }

  /// <summary>Whether the account is active and can login.</summary>
  public bool IsActive { get; set; }

  /// <summary>Whether the email has been verified via OTP.</summary>
  public bool IsEmailVerified { get; set; }

  /// <summary>JWT refresh token for session renewal.</summary>
  [MaxLength(500)]
  public string? RefreshToken { get; set; }

  /// <summary>Refresh token expiry date-time.</summary>
  public DateTime? RefreshTokenExpiry { get; set; }

  /// <summary>OTP code for email verification (6 digits). Null once verified.</summary>
  [MaxLength(10)]
  public string? VerificationToken { get; set; }

  /// <summary>OTP expiry date-time (typically 10 minutes from generation).</summary>
  public DateTime? VerificationTokenExpiry { get; set; }

  /// <summary>Account creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>Last profile update timestamp.</summary>
  public DateTime? UpdatedAt { get; set; }

  // Navigation properties
  public MembershipTier? MembershipTier { get; set; }
  public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
  public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
  public ICollection<Review> Reviews { get; set; } = new List<Review>();
  public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
  public ICollection<RecurringBooking> RecurringBookings { get; set; } = new List<RecurringBooking>();
  public ICollection<CoachSchedule> CoachSchedules { get; set; } = new List<CoachSchedule>();
  public ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();
  public ICollection<PlayerRequest> PlayerRequests { get; set; } = new List<PlayerRequest>();
  public ICollection<PlayerRequestMember> PlayerRequestMembers { get; set; } = new List<PlayerRequestMember>();
  public ICollection<Waitlist> Waitlists { get; set; } = new List<Waitlist>();
  public ICollection<MaintenanceSchedule> AssignedMaintenances { get; set; } = new List<MaintenanceSchedule>();
  public ICollection<CourtComplex> ManagedComplexes { get; set; } = new List<CourtComplex>();
}
