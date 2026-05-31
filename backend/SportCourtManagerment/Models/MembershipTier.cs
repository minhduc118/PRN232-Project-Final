using System.ComponentModel.DataAnnotations;

namespace SportCourtManagerment.Models;

/// <summary>Loyalty membership tier with discount benefits.</summary>
public class MembershipTier
{
  /// <summary>Primary key.</summary>
  public int TierId { get; set; }

  /// <summary>Tier name (Bronze / Silver / Gold / Platinum).</summary>
  [Required, MaxLength(50)]
  public string TierName { get; set; } = string.Empty;

  /// <summary>Minimum loyalty points required for this tier.</summary>
  public int MinPoints { get; set; }

  /// <summary>Discount percentage applied on bookings (0-100).</summary>
  public decimal DiscountPercent { get; set; }

  /// <summary>Tier benefits description.</summary>
  [MaxLength(300)]
  public string? Description { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public ICollection<User> Users { get; set; } = new List<User>();
}
