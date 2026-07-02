using System.ComponentModel.DataAnnotations;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.DTOs.Promotions;

public class CreatePromotionDto
{
    [Required(ErrorMessage = "Mã khuyến mãi không được để trống")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Mã khuyến mãi phải từ 4 đến 20 ký tự")]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Mã khuyến mãi chỉ chứa chữ in hoa và số, không khoảng trắng")]
    public string PromoCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên khuyến mãi không được để trống")]
    public string PromoName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DiscountType DiscountType { get; set; }

    [Required]
    public decimal DiscountValue { get; set; }

    public decimal MinOrderAmount { get; set; }

    public decimal? MaxDiscount { get; set; }

    public int? UsageLimit { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}
