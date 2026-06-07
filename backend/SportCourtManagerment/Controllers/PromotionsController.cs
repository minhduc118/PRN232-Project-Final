using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs.Promotions;
using SportCourtManagerment.Services.Promotions;

namespace SportCourtManagerment.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAllPromotions()
    {
        var result = await _promotionService.GetAllPromotionsAsync();
        return Ok(new { data = result, message = "Lấy danh sách thành công" });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetPromotionById(int id)
    {
        var result = await _promotionService.GetPromotionByIdAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy mã khuyến mãi." });
        return Ok(new { data = result });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _promotionService.CreatePromotionAsync(dto);
        if (!result.Success) return BadRequest(result);

        return CreatedAtAction(nameof(GetPromotionById), new { id = result.Data!.PromotionId }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePromotion(int id, [FromBody] UpdatePromotionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _promotionService.UpdatePromotionAsync(id, dto);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var result = await _promotionService.DeletePromotionAsync(id);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }
}
