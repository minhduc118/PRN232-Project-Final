using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Promotion;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class PromotionsController : ControllerBase
  {
    private readonly IPromotionService _promoService;

    public PromotionsController(IPromotionService promoService)
    {
      _promoService = promoService ?? throw new ArgumentNullException(nameof(promoService));
    }

    /// <summary>Gets paged promotions with optional filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PromotionFilterParams? filter)
    {
      try
      {
        filter ??= new PromotionFilterParams();
        var result = await _promoService.GetPagedPromotionsAsync(filter);
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Gets promotion by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
      try
      {
        var result = await _promoService.GetPromotionByIdAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy mã khuyến mãi." });
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Creates a new promotion.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest request)
    {
      try
      {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _promoService.CreatePromotionAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.PromotionId }, new { data = result, message = "Tạo khuyến mãi thành công." });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Updates an existing promotion.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePromotionRequest request)
    {
      try
      {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _promoService.UpdatePromotionAsync(id, request);
        if (result == null) return NotFound(new { message = "Không tìm thấy mã khuyến mãi." });
        return Ok(new { data = result, message = "Cập nhật khuyến mãi thành công." });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Deletes a promotion.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
      try
      {
        var deleted = await _promoService.DeletePromotionAsync(id);
        if (!deleted) return NotFound(new { message = "Không tìm thấy mã khuyến mãi." });
        return Ok(new { message = "Xóa khuyến mãi thành công." });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Validates coupon code and calculates discount.</summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
      try
      {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _promoService.ValidateCouponAsync(request);
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }
  }
}
