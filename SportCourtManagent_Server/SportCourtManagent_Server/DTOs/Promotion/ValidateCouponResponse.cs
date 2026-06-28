namespace SportCourtManagent_Server.DTOs.Promotion
{
  public class ValidateCouponResponse
  {
    public bool Valid { get; set; }
    public string? PromoCode { get; set; }
    public string? PromoName { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? Message { get; set; }
  }
}
