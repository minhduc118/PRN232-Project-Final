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
    [Route("api/courts")]
    public class CourtsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourtsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? complexId, [FromQuery] string? status)
        {
            var query = _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .Include(c => c.CourtImages)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (complexId.HasValue)
                query = query.Where(c => c.ComplexId == complexId.Value);

            if (!string.IsNullOrWhiteSpace(status) &&
                System.Enum.TryParse<SportCourtManagent_Server.Enums.CourtStatus>(status, true, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }

            var courts = await query
                .OrderBy(c => c.CourtName)
                .Select(c => MapToDto(c))
                .ToListAsync();

            return Ok(ApiResults.Ok(courts, "Lấy danh sách sân thành công."));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var court = await _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .Include(c => c.CourtImages)
                .FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);

            if (court == null)
                return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));

            return Ok(ApiResults.Ok(MapToDto(court), "Lấy thông tin sân thành công."));
        }

        private static CourtDto MapToDto(Court c) => new()
        {
            CourtId = c.CourtId,
            CourtName = c.CourtName,
            CourtCode = c.CourtCode,
            CourtTypeId = c.CourtTypeId,
            CourtTypeName = c.CourtType.TypeName,
            ComplexId = c.ComplexId,
            ComplexName = c.Complex.ComplexName,
            Status = c.Status.ToString(),
            OpenTime = c.OpenTime.ToString(@"hh\:mm"),
            CloseTime = c.CloseTime.ToString(@"hh\:mm"),
            PricePerHour = c.PricePerHour,
            CourtSize = c.CourtSize,
            ImageUrl = c.CourtImages.OrderBy(i => i.CourtImageId).Select(i => i.ImageUrl).FirstOrDefault()
        };
    }
}
