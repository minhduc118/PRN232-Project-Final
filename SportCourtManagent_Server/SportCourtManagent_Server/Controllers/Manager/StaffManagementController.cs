using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Staff;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers.Manager
{
    [Authorize]
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
        [Authorize(Roles = "Admin,Manager")]
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // POST /api/manager/complexes/{complexId}/staff/{staffId}/assign
        [HttpPost("{staffId:int}/assign")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignStaffToComplex([FromRoute] int complexId, [FromRoute] int staffId)
        {
            try
            {
                await _staffService.AssignStaffToComplexAsync(complexId, staffId);
                return Ok(new { Message = $"Nhân viên (Id={staffId}) đã được assign vào cơ sở (Id={complexId}) thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // DELETE /api/manager/complexes/{complexId}/staff/{staffId}/unassign
        [HttpDelete("{staffId:int}/unassign")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RemoveStaffFromComplex([FromRoute] int complexId, [FromRoute] int staffId)
        {
            try
            {
                await _staffService.RemoveStaffFromComplexAsync(complexId, staffId);
                return Ok(new { Message = $"Nhân viên (Id={staffId}) đã được gỡ khỏi cơ sở (Id={complexId}) thành công." });
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

        //  FR-ST-02: Xếp ca làm việc cho Staff

        // GET /api/manager/complexes/{complexId}/staff/shifts/weekly?weekStart=2026-06-02
        [HttpGet("shifts/weekly")]
        [Authorize(Roles = "Admin,Manager")]
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // GET /api/manager/complexes/{complexId}/staff/shifts/my?weekStart=2026-06-02
        [HttpGet("shifts/my")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetMyWeeklySchedule(
            [FromRoute] int complexId,
            [FromQuery] string? weekStart = null)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int staffId))
            {
                return Unauthorized(new { Message = "Không tìm thấy thông tin đăng nhập nhân viên." });
            }

            var date = weekStart != null && DateOnly.TryParse(weekStart, out var parsed)
              ? parsed
              : DateOnly.FromDateTime(DateTime.Today);

            try
            {
                var schedule = await _staffService.GetWeeklyScheduleByStaffAsync(complexId, staffId, date);
                return Ok(schedule);
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

        // GET /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
        [HttpGet("shifts/{shiftId:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetShiftById([FromRoute] int complexId, [FromRoute] int shiftId)
        {
            try
            {
                var shift = await _staffService.GetShiftByIdAsync(shiftId);
                return Ok(shift);
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

        // POST /api/manager/complexes/{complexId}/staff/shifts
        [HttpPost("shifts")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateShift(
            [FromRoute] int complexId,
            [FromBody] CreateShiftRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _staffService.CreateShiftAsync(complexId, request);
                return CreatedAtAction(nameof(GetShiftById), new { complexId = complexId, shiftId = created.ShiftId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }



        // PUT /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
        [HttpPut("shifts/{shiftId:int}")]
        [Authorize(Roles = "Admin,Manager")]
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // DELETE /api/manager/complexes/{complexId}/staff/shifts/{shiftId}
        [HttpDelete("shifts/{shiftId:int}")]
        [Authorize(Roles = "Admin,Manager")]
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // GET /api/manager/complexes/{complexId}/staff/attendance
        [HttpGet("attendance")]
        [Authorize(Roles = "Admin,Manager")]
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // POST /api/manager/complexes/{complexId}/staff/shifts/{shiftId}/check-in
        [HttpPost("shifts/{shiftId:int}/check-in")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManagerCheckIn([FromRoute] int complexId, [FromRoute] int shiftId)
        {
            try
            {
                var shift = await _staffService.GetShiftByIdAsync(shiftId);
                if (shift == null || shift.ComplexId != complexId)
                {
                    return NotFound(new { Message = "Không tìm thấy ca trực tại cơ sở này." });
                }

                var updated = await _staffService.CheckInShiftAsync(shift.StaffId, shiftId);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // POST /api/manager/complexes/{complexId}/staff/shifts/{shiftId}/check-out
        [HttpPost("shifts/{shiftId:int}/check-out")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManagerCheckOut([FromRoute] int complexId, [FromRoute] int shiftId)
        {
            try
            {
                var shift = await _staffService.GetShiftByIdAsync(shiftId);
                if (shift == null || shift.ComplexId != complexId)
                {
                    return NotFound(new { Message = "Không tìm thấy ca trực tại cơ sở này." });
                }

                var updated = await _staffService.CheckOutShiftAsync(shift.StaffId, shiftId);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
    }
}
