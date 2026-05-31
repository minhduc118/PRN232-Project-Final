namespace SportCourtManagerment.Models;

/// <summary>Price configuration for a specific court and time slot combination.</summary>
public class CourtPricing
{
  /// <summary>Primary key.</summary>
  public int PricingId { get; set; }

  /// <summary>FK to the court this price applies to.</summary>
  public int CourtId { get; set; }

  /// <summary>FK to the time slot this price applies to.</summary>
  public int SlotId { get; set; }

  /// <summary>Base price in VND.</summary>
  public decimal Price { get; set; }

  /// <summary>Peak-hour multiplier (1.0 = no extra, 1.5 = 50% extra).</summary>
  public decimal PeakMultiplier { get; set; }

  /// <summary>Date from which this pricing is effective.</summary>
  public DateOnly EffectiveFrom { get; set; }

  /// <summary>Optional date when this pricing expires (null = indefinite).</summary>
  public DateOnly? EffectiveTo { get; set; }

  /// <summary>Record creation timestamp.</summary>
  public DateTime CreatedAt { get; set; }

  // Navigation properties
  public Court Court { get; set; } = null!;
  public TimeSlot TimeSlot { get; set; } = null!;
}
