using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Staff;
using SportCourtManagerment.Services.Interface;

namespace SportCourtManagerment.Controllers.Manager;

[ApiController]
[Route("api/staff/shifts")]
[Authorize]
public class StaffShiftsController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffShiftsController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    // POST /api/staff/shifts/{shiftId}/check-in
    [HttpPost("{shiftId:int}/check-in")]
    public async Task<IActionResult> CheckIn(int shiftId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var staffId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được danh tính nhân viên.", 401));
        }

        try
        {
            var result = await _staffService.CheckInShiftAsync(staffId, shiftId);
            return Ok(ApiResponse<StaffShiftResponse>.Ok(result, "Check-in ca làm việc thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // POST /api/staff/shifts/{shiftId}/check-out
    [HttpPost("{shiftId:int}/check-out")]
    public async Task<IActionResult> CheckOut(int shiftId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var staffId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được danh tính nhân viên.", 401));
        }

        try
        {
            var result = await _staffService.CheckOutShiftAsync(staffId, shiftId);
            return Ok(ApiResponse<StaffShiftResponse>.Ok(result, "Check-out ca làm việc thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }
}
