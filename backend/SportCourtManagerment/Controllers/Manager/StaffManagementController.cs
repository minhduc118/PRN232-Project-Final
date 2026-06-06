using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Staff;
using SportCourtManagerment.Services.Interface;

namespace SportCourtManagerment.Controllers.Manager;

[ApiController]
[Route("api/manager/complexes/{complexId:int}/staff")]
[Authorize]
public class StaffManagementController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffManagementController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    // ════════════════════════════════════════════════════════════════
    //  FR-ST-01: Xem danh sách nhân sự khu vực
    // ════════════════════════════════════════════════════════════════

    // GET /api/manager/complexes/{complexId}/staff
    [HttpGet]
    public async Task<IActionResult> GetStaffList(
      int complexId,
      [FromQuery] string? search = null,
      [FromQuery] bool? isActive = null,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _staffService.GetStaffListAsync(complexId, search, isActive, page, pageSize);
            return Ok(ApiResponse<PagedStaffResponse>.Ok(result, "Lấy danh sách nhân sự thành công."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  FR-ST-02: Xếp ca làm việc cho Staff
    // ════════════════════════════════════════════════════════════════

    // GET /api/manager/complexes/{complexId}/staff/shifts/weekly?weekStart=2026-06-02
    [HttpGet("shifts/weekly")]
    public async Task<IActionResult> GetWeeklySchedule(
      int complexId,
      [FromQuery] string? weekStart = null)
    {
        var date = weekStart != null && DateOnly.TryParse(weekStart, out var parsed)
          ? parsed
          : DateOnly.FromDateTime(DateTime.Today);

        try
        {
            var schedule = await _staffService.GetWeeklyScheduleAsync(complexId, date);
            return Ok(ApiResponse<WeeklyScheduleResponse>.Ok(schedule, "Lấy lịch ca tuần thành công."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // GET /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
    [HttpGet("shifts/{shiftId:int}")]
    public async Task<IActionResult> GetShiftById(int complexId, int shiftId)
    {
        try
        {
            var shift = await _staffService.GetShiftByIdAsync(shiftId);
            return Ok(ApiResponse<StaffShiftResponse>.Ok(shift, "Lấy thông tin ca làm việc thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // POST /api/manager/complexes/{complexId}/staff/shifts
    [HttpPost("shifts")]
    public async Task<IActionResult> CreateShift(
      int complexId,
      [FromBody] CreateShiftRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
              "Dữ liệu đầu vào không hợp lệ.",
              errors: ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList()));

        try
        {
            var created = await _staffService.CreateShiftAsync(complexId, request);
            return CreatedAtAction(
              nameof(GetShiftById),
              new { complexId, shiftId = created.ShiftId },
              ApiResponse<StaffShiftResponse>.Created(created, "Tạo ca làm việc thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message, 409));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // POST /api/manager/complexes/{complexId}/staff/shifts/bulk
    [HttpPost("shifts/bulk")]
    public async Task<IActionResult> CreateShiftBulk(
      int complexId,
      [FromBody] BulkCreateShiftRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ."));

        try
        {
            var result = await _staffService.CreateShiftBulkAsync(complexId, request);
            var message = $"Tạo thành công {result.Created} ca. Bỏ qua {result.Skipped} ca do trùng hoặc lỗi.";
            return Ok(ApiResponse<BulkCreateShiftResponse>.Ok(result, message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // PUT /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
    [HttpPut("shifts/{shiftId:int}")]
    public async Task<IActionResult> UpdateShift(
      int complexId,
      int shiftId,
      [FromBody] UpdateShiftRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ."));

        try
        {
            var updated = await _staffService.UpdateShiftAsync(complexId, shiftId, request);
            return Ok(ApiResponse<StaffShiftResponse>.Ok(updated, "Cập nhật ca làm việc thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message, 409));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // DELETE /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
    [HttpDelete("shifts/{shiftId:int}")]
    public async Task<IActionResult> DeleteShift(int complexId, int shiftId)
    {
        try
        {
            await _staffService.DeleteShiftAsync(complexId, shiftId);
            return Ok(ApiResponse<object>.Ok(null, "Xóa ca làm việc thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
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
