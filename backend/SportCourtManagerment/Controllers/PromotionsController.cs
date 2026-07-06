using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Promotions;

namespace SportCourtManagerment.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PromotionsController : ControllerBase
{
    private readonly SportCourtManagerment.Services.Promotions.IPromotionService _promotionService;
    private readonly SportCourtManagerment.Services.Interfaces.IPromotionService? _activePromotionService;

    public PromotionsController(SportCourtManagerment.Services.Promotions.IPromotionService promotionService, IEnumerable<SportCourtManagerment.Services.Interfaces.IPromotionService> activePromotionServices)
    {
        _promotionService = promotionService;
        _activePromotionService = activePromotionServices.FirstOrDefault();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPromotions()
    {
        var result = await _promotionService.GetAllPromotionsAsync();
        return Ok(new { data = result, message = "Lấy danh sách thành công" });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPromotionById(int id)
    {
        var result = await _promotionService.GetPromotionByIdAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy mã khuyến mãi." });
        return Ok(new { data = result });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _promotionService.CreatePromotionAsync(dto);
        if (!result.Success) return BadRequest(result);

        return CreatedAtAction(nameof(GetPromotionById), new { id = result.Data!.PromotionId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePromotion(int id, [FromBody] UpdatePromotionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _promotionService.UpdatePromotionAsync(id, dto);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var result = await _promotionService.DeletePromotionAsync(id);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActivePromotions()
    {
        if (_activePromotionService != null)
        {
            var promotions = await _activePromotionService.GetActivePromotionsAsync();
            return Ok(ApiResponse<List<PromotionDto>>.Ok(promotions, "Lấy danh sách khuyến mãi thành công."));
        }
        return Ok(new { data = new List<PromotionDto>() });
    }
}
