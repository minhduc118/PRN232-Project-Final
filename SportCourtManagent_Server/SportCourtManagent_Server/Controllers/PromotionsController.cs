using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Promotion;
using SportCourtManagent_Server.Services.Interfaces;
using SportCourtManagent_Server.Helpers;

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
        return Ok(ApiResults.Ok(result));
      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }

    /// <summary>Gets promotion by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
      try
      {
        var result = await _promoService.GetPromotionByIdAsync(id);
        if (result == null) return NotFound(ApiResults.Fail("Không tìm thấy mã khuyến mãi.", 404));
        return Ok(ApiResults.Ok(result));
      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }

    /// <summary>Creates a new promotion.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest request)
    {
      try
      {
        if (!ModelState.IsValid) return BadRequest(ApiResults.Fail("Dữ liệu không hợp lệ.", 400));
        var result = await _promoService.CreatePromotionAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.PromotionId }, ApiResults.Ok(result, "Tạo khuyến mãi thành công.", 201));
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ApiResults.Fail(ex.Message, 400));
      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }

    /// <summary>Updates an existing promotion.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePromotionRequest request)
    {
      try
      {
        if (!ModelState.IsValid) return BadRequest(ApiResults.Fail("Dữ liệu không hợp lệ.", 400));
        var result = await _promoService.UpdatePromotionAsync(id, request);
        if (result == null) return NotFound(ApiResults.Fail("Không tìm thấy mã khuyến mãi.", 404));
        return Ok(ApiResults.Ok(result, "Cập nhật khuyến mãi thành công."));
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
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
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
        if (!deleted) return NotFound(ApiResults.Fail("Không tìm thấy mã khuyến mãi.", 404));
        return Ok(ApiResults.Ok(null, "Xóa khuyến mãi thành công."));
      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }

    /// <summary>Validates coupon code and calculates discount.</summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
    {
      try
      {
        if (!ModelState.IsValid) return BadRequest(ApiResults.Fail("Dữ liệu không hợp lệ.", 400));
        var result = await _promoService.ValidateCouponAsync(request);
        return Ok(ApiResults.Ok(result));
      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }
  }
}
