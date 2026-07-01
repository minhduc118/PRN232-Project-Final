using System;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffShiftController : ControllerBase
    {
        private readonly IStaffShiftService _staffShiftService;

        public StaffShiftController(IStaffShiftService staffShiftService)
        {
            _staffShiftService = staffShiftService;
        }

        [HttpGet("today")]
        public IActionResult GetTodayShifts()
        {
            try
            {
                var shifts = _staffShiftService.GetTodayShifts();
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkin/{id}")]
        public IActionResult CheckIn(int id, [FromBody] CheckInRequest request)
        {
            try
            {
                var shift = _staffShiftService.CheckIn(id, request.PhotoBase64);
                if (shift == null)
                {
                    return NotFound(new { message = $"Shift with ID {id} not found." });
                }
                return Ok(shift);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkout/{id}")]
        public IActionResult CheckOut(int id, [FromBody] CheckInRequest request)
        {
            try
            {
                var shift = _staffShiftService.CheckOut(id, request.PhotoBase64);
                if (shift == null)
                {
                    return NotFound(new { message = $"Shift with ID {id} not found." });
                }
                return Ok(shift);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("seed")]
        public IActionResult SeedData()
        {
            try
            {
                _staffShiftService.SeedDemoData();
                return Ok(new { message = "Demo roles, users, and today's shifts have been seeded successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CheckInRequest
    {
        public string PhotoBase64 { get; set; } = string.Empty;
    }
}
