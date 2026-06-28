using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Staff;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers.Manager
{
    [Route("api/manager/complexes/{complexId:int}/staff")]
    [ApiController]
    public class StaffManagementController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffManagementController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        //  FR-ST-01: Xem danh sách nhân sự khu vực

        // GET /api/manager/complexes/{complexId}/staff
        [HttpGet]
        public async Task<IActionResult> GetStaffList(
            int complexId,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _staffService.GetStaffListAsync(complexId, search, isActive, page, pageSize);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi hệ thống");
            }
        }

        //  FR-ST-02: Xếp ca làm việc cho Staff

        // GET /api/manager/complexes/{complexId}/staff/shifts/weekly?weekStart=2026-06-02
        [HttpGet("shifts/weekly")]
        public async Task<IActionResult> GetWeeklySchedule(
            [FromRoute] int complexId,
            [FromQuery] string? weekStart = null)
        {
            var date = weekStart != null && DateOnly.TryParse(weekStart, out var parsed)
              ? parsed
              : DateOnly.FromDateTime(DateTime.Today);

            try
            {
                var schedule = await _staffService.GetWeeklyScheduleAsync(complexId, date);
                return Ok(schedule);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi hệ thống");
            }
        }

        // GET /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
        [HttpGet("shifts/{shiftId:int}")]
        public async Task<IActionResult> GetShiftById([FromRoute] int complexId, [FromRoute] int shiftId)
        {
            try
            {
                var shift = await _staffService.GetShiftByIdAsync(shiftId);
                return Ok(shift);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi hệ thống");
            }
        }

        // POST /api/manager/complexes/{complexId}/staff/shifts
        [HttpPost("shifts")]
        public async Task<IActionResult> CreateShift(
            [FromRoute] int complexId,
            [FromBody] CreateShiftRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var created = await _staffService.CreateShiftAsync(complexId, request);
                return CreatedAtAction(nameof(GetShiftById), new { complexId = complexId, shiftId = created.ShiftId }, created);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi hệ thống");
            }
        }

        // POST /api/manager/complexes/{complexId}/staff/shifts/bulk
        [HttpPost("shifts/bulk")]
        public async Task<IActionResult> CreateShiftBulk(
            [FromRoute] int complexId,
            [FromBody] BulkCreateShiftRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var result = await _staffService.CreateShiftBulkAsync(complexId, request);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi hệ thống");
            }
        }

        // PUT /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
        [HttpPut("shifts/{shiftId:int}")]
        public async Task<IActionResult> UpdateShift(
            [FromRoute] int complexId,
            [FromRoute] int shiftId,
            [FromBody] UpdateShiftRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _staffService.UpdateShiftAsync(complexId, shiftId, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // DELETE /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
        [HttpDelete("shifts/{shiftId:int}")]
        public async Task<IActionResult> DeleteShift([FromRoute] int complexId, [FromRoute] int shiftId)
        {
            try
            {
                await _staffService.DeleteShiftAsync(complexId, shiftId);
                return Ok(new { Message = "Xóa ca làm việc thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // GET /api/manager/complexes/{complexId}/staff/attendance
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceReport(
            int complexId,
            [FromQuery] string? dateFrom = null,
            [FromQuery] string? dateTo = null,
            [FromQuery] int? staffId = null)
        {
            DateOnly? from = null;
            if (DateOnly.TryParse(dateFrom, out var parsedFrom))
            {
                from = parsedFrom;
            }

            DateOnly? to = null;
            if (DateOnly.TryParse(dateTo, out var parsedTo))
            {
                to = parsedTo;
            }

            try
            {
                var report = await _staffService.GetAttendanceReportAsync(complexId, from, to, staffId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
    }
}
