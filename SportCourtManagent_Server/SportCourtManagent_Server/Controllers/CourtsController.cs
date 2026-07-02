using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/courts")]
    public class CourtsController : ControllerBase
    {
        private readonly ICourtService _courtService;

        public CourtsController(ICourtService courtService)
        {
            _courtService = courtService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? complexId, [FromQuery] string? status)
        {
            var courts = await _courtService.GetAllAsync(complexId, status);
            return Ok(ApiResults.Ok(courts, "Lấy danh sách sân thành công."));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var court = await _courtService.GetByIdAsync(id);
            if (court == null)
                return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));
            return Ok(ApiResults.Ok(court, "Lấy thông tin sân thành công."));
        }
    }
}
