using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    
    [ApiController]
    [Route("api/courts")]
    public class CourtsController : ODataController
    {
        private readonly ICourtService _courtService;

        public CourtsController(ICourtService courtService)
        {
            _courtService = courtService;
        }

        // GET /api/courts — Search with filters + pagination (REST)
        [HttpGet]
        public async Task<IActionResult> SearchCourts([FromQuery] CourtSearchParams searchParams)
        {
            var result = await _courtService.SearchCourtsAsync(searchParams);
            return Ok(new { data = result });
        }

        // GET /api/courts/{id} — Court detail
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCourtDetail(int id)
        {
            var detail = await _courtService.GetCourtDetailAsync(id);
            if (detail is null)
                return NotFound(new { message = "Không tìm thấy sân." });

            return Ok(new { data = detail });
        }

        // GET /api/courts/{id}/availability?date=YYYY-MM-DD
        [HttpGet("{id:int}/availability")]
        public async Task<IActionResult> GetCourtAvailability(int id, [FromQuery] DateTime date)
        {
            var availability = await _courtService.GetCourtAvailabilityAsync(id, date);
            if (availability is null)
                return NotFound(new { message = "Không tìm thấy sân." });

            return Ok(new { data = availability });
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
