using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs.Bookings;
using SportCourtManagerment.Services.Bookings;

namespace SportCourtManagerment.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("admin")]
    // [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminBookings([FromQuery] DateOnly? date, [FromQuery] int? courtTypeId, [FromQuery] string? status)
    {
        var result = await _bookingService.GetAdminBookingsAsync(date, courtTypeId, status);
        return Ok(new { data = result, message = "Lấy danh sách đặt sân thành công" });
    }

    [HttpGet("{id}")]
    // [Authorize]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var result = await _bookingService.GetBookingByIdAsync(id);
        if (result == null) return NotFound(new { message = "Không tìm thấy đơn đặt sân." });
        return Ok(new { data = result });
    }

    [HttpPost("admin")]
    // [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateBookingFromAdmin([FromBody] CreateBookingAdminDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _bookingService.CreateBookingFromAdminAsync(dto);
        if (!result.Success) return BadRequest(result);

        return CreatedAtAction(nameof(GetBookingById), new { id = result.Data!.BookingId }, result);
    }

    [HttpPut("{id}/status")]
    // [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _bookingService.UpdateBookingStatusAsync(id, dto);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }
}
