using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/court-types")]
    public class CourtTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourtTypesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var types = await _context.CourtTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.TypeName)
                .Select(t => new CourtTypeDto
                {
                    CourtTypeId = t.CourtTypeId,
                    TypeName = t.TypeName,
                    IsActive = t.IsActive,
                    CourtCount = t.Courts != null ? t.Courts.Count : 0,
                    IconUrl = "",
                    Description = ""
                })
                .ToListAsync();

            return Ok(ApiResults.Ok(types, "Lấy danh sách loại sân thành công."));
        }
    }
}
