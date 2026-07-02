using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/court-types")]
    public class CourtTypesController : ControllerBase
    {
        private readonly ICourtTypeService _courtTypeService;

        public CourtTypesController(ICourtTypeService courtTypeService)
        {
            _courtTypeService = courtTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var types = await _courtTypeService.GetAllActiveAsync();
            return Ok(ApiResults.Ok(types, "Lấy danh sách loại sân thành công."));
        }
    }
}
