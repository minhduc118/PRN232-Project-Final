using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Services.Interfaces;
using SportCourtManagent_Server.DataAccess.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    
    [ApiController]
    [Route("api/courts")]
    public class CourtsController : ODataController
    {
        private readonly ICourtService _courtService;
        private readonly ICourtTypeRepository _courtTypeRepo;
        private readonly ITimeSlotRepository _timeSlotRepo;

        public CourtsController(ICourtService courtService, ICourtTypeRepository courtTypeRepo, ITimeSlotRepository timeSlotRepo)
        {
            _courtService = courtService;
            _courtTypeRepo = courtTypeRepo;
            _timeSlotRepo = timeSlotRepo;
        }

        // GET /api/courts — Search with filters + pagination (REST)
        [HttpGet]
        public async Task<IActionResult> SearchCourts([FromQuery] CourtSearchParams searchParams)
        {
            var result = await _courtService.SearchCourtsAsync(searchParams);
            return Ok(new { success = true, data = result });
        }

        // GET /api/court-types
        [HttpGet("/api/court-types")]
        public IActionResult GetCourtTypes()
        {
            var types = _courtTypeRepo.GetAll().Select(ct => new
            {
                courtTypeId = ct.CourtTypeId,
                typeName = ct.TypeName,
                iconUrl = "",
                description = "",
                courtCount = ct.Courts?.Count ?? 0
            });
            return Ok(new { success = true, data = types });
        }

        // GET /api/time-slots
        [HttpGet("/api/time-slots")]
        public async Task<IActionResult> GetTimeSlots()
        {
            var slots = await _timeSlotRepo.GetAllAsync();
            var result = slots.Select(s => new
            {
                slotId = s.SlotId,
                slotName = s.SlotName,
                startTime = s.StartTime.ToString(@"hh\:mm"),
                endTime = s.EndTime.ToString(@"hh\:mm"),
                dayType = s.DayType.ToString()
            }).OrderBy(s => s.startTime);
            return Ok(new { success = true, data = result });
        }

        // GET /api/courts/{id} — Court detail
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCourtDetail(int id)
        {
            var detail = await _courtService.GetCourtDetailAsync(id);
            if (detail is null)
                return NotFound(new { success = false, message = "Không tìm thấy sân." });

            return Ok(new { success = true, data = detail });
        }

        // GET /api/courts/{id}/availability?date=YYYY-MM-DD
        [HttpGet("{id:int}/availability")]
        public async Task<IActionResult> GetCourtAvailability(int id, [FromQuery] DateTime date)
        {
            var availability = await _courtService.GetCourtAvailabilityAsync(id, date);
            if (availability is null)
                return NotFound(new { success = false, message = "Không tìm thấy sân." });

            return Ok(new { success = true, data = availability });
        }

        // GET /odata/courts — OData query
        [HttpGet("/odata/courts")]
        [EnableQuery(MaxTop = 100, PageSize = 50)]
        public IActionResult GetOData()
        {
            return Ok(_courtService.GetCourtsODataQueryable());
        }
    }
}
