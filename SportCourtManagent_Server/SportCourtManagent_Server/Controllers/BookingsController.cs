using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Booking;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class BookingsController : ControllerBase
  {
    private readonly IBookingManagementService _bookingService;

    public BookingsController(IBookingManagementService bookingService)
    {
      _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
    }

    /// <summary>Gets bookings for logged in customer.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
      try
      {
        if (!TryGetUserId(out int userId)) return Unauthorized(new { message = "Không xác định được người dùng." });
        var result = await _bookingService.GetCustomerBookingsAsync(userId);
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Gets all bookings for Admin and Staff.</summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminBookings([FromQuery] DateTime? date, [FromQuery] int? courtTypeId, [FromQuery] string? status)
    {
      try
      {
        var result = await _bookingService.GetAdminBookingsAsync(date, courtTypeId, status);
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Gets booking detail by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
      try
      {
        var result = await _bookingService.GetBookingDetailAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy đơn đặt sân." });
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Creates a booking for customer.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
      try
      {
        if (!TryGetUserId(out int userId)) return Unauthorized(new { message = "Không xác định được người dùng." });
        var result = await _bookingService.CreateBookingAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.BookingId }, new { data = result, message = "Đặt sân thành công." });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Creates a booking from admin dashboard.</summary>
    [HttpPost("admin")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateFromAdmin([FromBody] CreateBookingRequest request)
    {
      try
      {
        if (!TryGetUserId(out int userId)) return Unauthorized(new { message = "Không xác định được người dùng." });
        var result = await _bookingService.CreateBookingAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.BookingId }, new { data = result, message = "Tạo đơn đặt sân thành công." });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Updates booking status from Admin or Staff.</summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusRequest request)
    {
      try
      {
        var result = await _bookingService.UpdateBookingStatusAsync(id, request);
        if (result == null) return NotFound(new { message = "Không tìm thấy đơn đặt sân." });
        return Ok(new { data = result, message = "Cập nhật trạng thái thành công." });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Blocks customer self cancellation.</summary>
    [HttpPut("{id}/cancel")]
    public IActionResult CancelBooking(int id)
    {
      return BadRequest(new { message = "Quy định hệ thống: Khách hàng không được phép hủy sân. Vui lòng liên hệ nhân viên hỗ trợ nếu bạn cần đổi sân hoặc khung giờ." });
    }

    /// <summary>Helper method to retrieve current logged in userId.</summary>
    private bool TryGetUserId(out int userId)
    {
      userId = 0;
      var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      return !string.IsNullOrEmpty(claim) && int.TryParse(claim, out userId);
    }
  }
}
