using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Models;

/// <summary>Discount promotion with promo code and validity period.</summary>
public class Promotion
{
  /// <summary>Primary key.</summary>
  public int PromotionId { get; set; }

  /// <summary>Unique promo code entered by customers.</summary>
  [Required, MaxLength(50)]
  public string PromoCode { get; set; } = string.Empty;

  /// <summary>Campaign display name.</summary>
  [Required, MaxLength(200)]
  public string PromoName { get; set; } = string.Empty;

  /// <summary>Campaign description for marketing display.</summary>
  [MaxLength(500)]
  public string? Description { get; set; }

  /// <summary>How the discount is calculated.</summary>
  public DiscountType DiscountType { get; set; }

  /// <summary>Discount value (percent or fixed VND).</summary>
  public decimal DiscountValue { get; set; }

  /// <summary>Minimum order amount required to apply promo.</summary>
  public decimal MinOrderAmount { get; set; }

  /// <summary>Maximum discount cap for percentage-type promos.</summary>
  public decimal? MaxDiscount { get; set; }

  /// <summary>Total number of times this promo can be used (null = unlimited).</summary>
  public int? UsageLimit { get; set; }

  /// <summary>Current number of times the promo has been used.</summary>
  public int UsedCount { get; set; }

  /// <summary>Promo validity start date.</summary>
  public DateTime StartDate { get; set; }

  /// <summary>Promo validity end date.</summary>
  public DateTime EndDate { get; set; }

  /// <summary>Whether the promo is currently active.</summary>
  public bool IsActive { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
