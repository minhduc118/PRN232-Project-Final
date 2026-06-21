namespace SportCourtManagerment.DTOs.Promotions;

/// <summary>
/// DTO for active promotions displayed on landing page and booking flow.
/// Sensitive fields (UsageLimit, UsedCount) are excluded.
/// </summary>
public class PromotionDto
{
  public int PromotionId { get; set; }
  public string PromoCode { get; set; } = string.Empty;
  public string PromoName { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string DiscountType { get; set; } = string.Empty;
  public decimal DiscountValue { get; set; }
  public decimal MinOrderAmount { get; set; }
  public decimal? MaxDiscount { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
}
