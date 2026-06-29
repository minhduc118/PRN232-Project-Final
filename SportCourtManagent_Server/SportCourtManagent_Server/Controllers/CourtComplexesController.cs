using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/complexes")]
    public class CourtComplexesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourtComplexesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int? courtTypeId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 8)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);

            var query = _context.CourtComplexes
                .Include(cx => cx.Courts)
                .Include(cx => cx.Manager)
                .Where(cx => !cx.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(cx =>
                    cx.ComplexName.Contains(term) ||
                    cx.Address.Contains(term) ||
                    (cx.Manager != null && cx.Manager.FullName.Contains(term)));
            }

            if (courtTypeId.HasValue)
            {
                query = query.Where(cx =>
                    cx.Courts.Any(c => !c.IsDeleted && c.CourtTypeId == courtTypeId.Value));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(cx => cx.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cx => MapToDto(cx))
                .ToListAsync();

            var stats = await GetStatsInternalAsync();
            var result = new PagedComplexResult
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Stats = stats
            };

            return Ok(ApiResults.Ok(result, "Lấy danh sách tổ hợp sân thành công."));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await GetStatsInternalAsync();
            return Ok(ApiResults.Ok(stats, "Lấy thống kê thành công."));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cx = await _context.CourtComplexes
                .Include(c => c.Courts)
                .Include(c => c.Manager)
                .FirstOrDefaultAsync(c => c.ComplexId == id && !c.IsDeleted);

            if (cx == null)
                return NotFound(ApiResults.Fail("Không tìm thấy tổ hợp sân.", 404));

            return Ok(ApiResults.Ok(MapToDto(cx), "Lấy thông tin tổ hợp sân thành công."));
        }

        private async Task<ComplexStatsDto> GetStatsInternalAsync()
        {
            return new ComplexStatsDto
            {
                TotalComplexes = await _context.CourtComplexes.CountAsync(cx => !cx.IsDeleted),
                TotalCourts = await _context.Courts.CountAsync(c => !c.IsDeleted),
                ActiveCourts = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Available),
                MaintenanceCourts = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Maintenance),
                InactiveCourts = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Inactive)
            };
        }

        private static CourtComplexDto MapToDto(CourtComplex cx) => new()
        {
            ComplexId = cx.ComplexId,
            ComplexName = cx.ComplexName,
            Address = cx.Address,
            Phone = cx.Manager?.Phone,
            ManagerName = cx.Manager?.FullName,
            ManagerId = cx.ManagerId,
            Description = cx.Description,
            ImageUrl = cx.ImageUrl,
            TotalCourts = cx.Courts.Count(c => !c.IsDeleted),
            ActiveCourts = cx.Courts.Count(c => !c.IsDeleted && c.Status == CourtStatus.Available),
            MaintenanceCourts = cx.Courts.Count(c => !c.IsDeleted && c.Status == CourtStatus.Maintenance),
            InactiveCourts = cx.Courts.Count(c => !c.IsDeleted && c.Status == CourtStatus.Inactive),
            CourtTypeIds = cx.Courts.Where(c => !c.IsDeleted).Select(c => c.CourtTypeId).Distinct().ToList(),
            CreatedAt = cx.CreatedAt
        };
    }
}
