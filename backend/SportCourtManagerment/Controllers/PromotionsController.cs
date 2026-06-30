using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Promotions;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Controllers;

[ApiController]
[Route("api/promotions")]
public class PromotionsController : ControllerBase
{
  private readonly IPromotionService _promotionService;

  public PromotionsController(IPromotionService promotionService)
  {
    _promotionService = promotionService;
  }


  //  GET /api/promotions/active

  [HttpGet("active")]
  public async Task<IActionResult> GetActivePromotions()
  {
    var promotions = await _promotionService.GetActivePromotionsAsync();
    return Ok(ApiResponse<List<PromotionDto>>.Ok(promotions,
      "Lấy danh sách khuyến mãi thành công."));
  }
}
