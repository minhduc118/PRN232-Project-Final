using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Booking;
using SportCourtManagent_Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Helpers;


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

    /// <summary>Gets paged bookings for logged in customer.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings([FromQuery] BookingFilterParams? filter)
    {
      try
      {
        filter ??= new BookingFilterParams();
        if (!TryGetUserId(out int userId)) return Unauthorized(ApiResults.Fail("Không xác định được người dùng.", 401));
        var result = await _bookingService.GetPagedCustomerBookingsAsync(userId, filter);
        return Ok(ApiResults.Ok(result));

      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }

    /// <summary>Gets all bookings for Admin and Staff.</summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminBookings([FromQuery] BookingFilterParams? filter)
    {
      try
      {
        filter ??= new BookingFilterParams();
        var result = await _bookingService.GetPagedAdminBookingsAsync(filter);
        return Ok(ApiResults.Ok(result));

      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }

    /// <summary>Gets booking detail by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
      try
      {
        var result = await _bookingService.GetBookingDetailAsync(id);
        if (result == null) return NotFound(ApiResults.Fail("Không tìm thấy đơn đặt sân.", 404));
        return Ok(ApiResults.Ok(result));
      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }

    /// <summary>Creates a booking for customer.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
      try
      {
        if (!TryGetUserId(out int userId)) return Unauthorized(ApiResults.Fail("Không xác định được người dùng.", 401));
        var result = await _bookingService.CreateBookingAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.BookingId }, ApiResults.Ok(result, "Đặt sân thành công.", 201));
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ApiResults.Fail(ex.Message, 400));
      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
      }
    }

    /// <summary>Creates recurring bookings for customer.</summary>
    [HttpPost("recurring")]
    public async Task<IActionResult> CreateRecurring([FromBody] CreateRecurringBookingRequest request)
    {
      try
      {
        if (!TryGetUserId(out int userId)) return Unauthorized(ApiResults.Fail("Không xác định được người dùng.", 401));
        var result = await _bookingService.CreateRecurringBookingAsync(userId, request);
        return Ok(ApiResults.Ok(result, "Đặt sân định kỳ thành công."));
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ApiResults.Fail(ex.Message, 400));
      }
      catch (Exception ex)
      {
        return StatusCode(500, ApiResults.Fail(ex.Message, 500));
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

    /// <summary>Creates a tournament booking (multiple courts and slots).</summary>
    [HttpPost("tournament")]
    public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentRequest request)
    {
      try
      {
        if (!TryGetUserId(out int userId)) return Unauthorized(new { message = "Không xác định được người dùng." });
        var result = await _bookingService.CreateTournamentAsync(userId, request);
        return Ok(new { data = result, message = "Tạo giải đấu thành công." });
      }
      catch (ArgumentException ex)
      {
        return Conflict(new { message = ex.Message });
      }
      catch (DbUpdateException)
      {
        return Conflict(new { message = "Khung giờ thi đấu này vừa có người khác đặt thành công trước bạn vài giây. Vui lòng chọn ca khác." });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Gets tournament list for the current logged-in customer.</summary>
    [HttpGet("tournament/my")]
    public async Task<IActionResult> GetMyTournaments([FromQuery] TournamentFilterParams? filter)
    {
      try
      {
        filter ??= new TournamentFilterParams();
        if (!TryGetUserId(out int userId)) return Unauthorized(new { message = "Không xác định được người dùng." });
        var result = await _bookingService.GetPagedCustomerTournamentsAsync(userId, filter);
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Gets all tournaments with optional filters for Admin and Staff.</summary>
    [HttpGet("tournament/admin")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAdminTournaments([FromQuery] TournamentFilterParams? filter)
    {
      try
      {
        filter ??= new TournamentFilterParams();
        var result = await _bookingService.GetPagedAdminTournamentsAsync(filter);
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Gets public tournaments list for all customers and visitors.</summary>
    [HttpGet("tournament/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicTournaments([FromQuery] TournamentFilterParams? filter)
    {
      try
      {
        filter ??= new TournamentFilterParams();
        var result = await _bookingService.GetPagedPublicTournamentsAsync(filter);
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Gets tournament detail with optional filters for Customer but not owner.</summary>
    [HttpGet("tournament/{id}/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTournamentPublic([FromRoute] int id)
    {
      try
      {
        var result = await _bookingService.GetTournamentPublicInfoAsync(id);
        return Ok(new { data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }
    /// <summary>Gets tournament detail by ID. Customer can only view their own tournament.</summary>
    [HttpGet("tournament/{id:int}")]
    public async Task<IActionResult> GetTournamentById(int id)
    {
      try
      {
        if (!TryGetUserId(out int userId)) return Unauthorized(new { message = "Không xác định được người dùng." });
        bool isAdminOrStaff = User.IsInRole("Admin") || User.IsInRole("Staff");
        var result = await _bookingService.GetTournamentDetailAsync(id, userId, isAdminOrStaff);
        if (result == null) return NotFound(new { message = "Không tìm thấy giải đấu." });
        return Ok(new { data = result });
      }
      catch (UnauthorizedAccessException)
      {
        return Forbid();
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Updates tournament status (Admin/Staff only). Cancelling cascades to all child bookings.</summary>
    [HttpPut("tournament/{id:int}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateTournamentStatus(int id, [FromBody] UpdateTournamentStatusRequest request)
    {
      try
      {
        var result = await _bookingService.UpdateTournamentStatusAsync(id, request);
        if (result == null) return NotFound(new { message = "Không tìm thấy giải đấu." });
        return Ok(new { data = result, message = "Cập nhật trạng thái giải đấu thành công." });
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

    /// <summary>Updates tournament info, courts, slots and services (Customer only, within 24h of creation).</summary>
    [HttpPut("tournament/{id:int}/info")]
    public async Task<IActionResult> UpdateTournamentInfo(int id, [FromBody] UpdateTournamentInfoRequest request)
    {
      try
      {
        if (!TryGetUserId(out int userId)) return Unauthorized(new { message = "Không xác định được người dùng." });
        var result = await _bookingService.UpdateTournamentInfoAsync(id, userId, request);
        if (result == null) return NotFound(new { message = "Không tìm thấy giải đấu." });
        return Ok(new { data = result, message = "Cập nhật thông tin giải đấu thành công." });
      }
      catch (UnauthorizedAccessException)
      {
        return Forbid();
      }
      catch (ArgumentException ex)
      {
        return Conflict(new { message = ex.Message });
      }
      catch (DbUpdateException)
      {
        return Conflict(new { message = "Khung giờ thi đấu mới bạn chọn vừa có người khác đặt thành công. Vui lòng chọn ca khác." });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    /// <summary>Updates booking status from Admin or Staff.</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusRequest request)
    {
      try
      {
        var result = await _bookingService.UpdateBookingStatusAsync(id, request);
        if (result == null) return NotFound(new { message = "Không tìm thấy đơn đặt sân." });
        return Ok(new { data = result, message = "Cập nhật trạng thái thành công." });
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

    /// <summary>Blocks customer self cancellation.</summary>
    [HttpPut("{id:int}/cancel")]
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
