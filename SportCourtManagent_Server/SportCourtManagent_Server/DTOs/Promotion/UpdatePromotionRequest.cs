using System;
using System.ComponentModel.DataAnnotations;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.DTOs.Promotion
{
  public class UpdatePromotionRequest
  {
    [Required]
    [MaxLength(100)]
    public string PromoName { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DiscountType DiscountType { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinOrderAmount { get; set; } = 0;

    [Range(0.01, double.MaxValue, ErrorMessage = "Max discount must be greater than 0")]
    public decimal? MaxDiscount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be greater than 0")]
    public int? UsageLimit { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
  }
}
