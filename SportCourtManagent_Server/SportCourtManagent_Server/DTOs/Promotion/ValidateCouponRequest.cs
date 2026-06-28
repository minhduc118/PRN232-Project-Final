using System.ComponentModel.DataAnnotations;

namespace SportCourtManagent_Server.DTOs.Promotion
{
  public class ValidateCouponRequest
  {
    [Required]
    public string PromoCode { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal OrderAmount { get; set; }
  }
}
