using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] UpsertCourtComplexRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ComplexName))
                return BadRequest(ApiResults.Fail("Tên tổ hợp không được để trống."));
            if (string.IsNullOrWhiteSpace(request.Address))
                return BadRequest(ApiResults.Fail("Địa chỉ không được để trống."));
            if (request.ManagerId <= 0)
                return BadRequest(ApiResults.Fail("Vui lòng chọn quản lý phụ trách."));

            var manager = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == request.ManagerId && u.IsActive);
            if (manager == null)
                return BadRequest(ApiResults.Fail("Quản lý không tồn tại hoặc đã bị vô hiệu hóa."));
            if (!manager.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
                return BadRequest(ApiResults.Fail("Người được chọn phải có vai trò Staff."));

            var complex = new CourtComplex
            {
                ComplexName = request.ComplexName.Trim(),
                Address = request.Address.Trim(),
                ManagerId = request.ManagerId,
                Description = request.Description?.Trim(),
                ImageUrl = request.ImageUrl?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.CourtComplexes.Add(complex);
            await _context.SaveChangesAsync();

            await _context.Entry(complex).Reference(c => c.Manager).LoadAsync();
            await _context.Entry(complex).Collection(c => c.Courts).LoadAsync();

            return StatusCode(201, ApiResults.Ok(MapToDto(complex), "Tạo tổ hợp sân thành công.", 201));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertCourtComplexRequest request)
        {
            var complex = await _context.CourtComplexes
                .Include(c => c.Courts)
                .Include(c => c.Manager)
                .FirstOrDefaultAsync(c => c.ComplexId == id && !c.IsDeleted);

            if (complex == null)
                return NotFound(ApiResults.Fail("Không tìm thấy tổ hợp sân.", 404));

            if (string.IsNullOrWhiteSpace(request.ComplexName))
                return BadRequest(ApiResults.Fail("Tên tổ hợp không được để trống."));
            if (string.IsNullOrWhiteSpace(request.Address))
                return BadRequest(ApiResults.Fail("Địa chỉ không được để trống."));
            if (request.ManagerId <= 0)
                return BadRequest(ApiResults.Fail("Vui lòng chọn quản lý phụ trách."));

            var manager = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == request.ManagerId && u.IsActive);
            if (manager == null)
                return BadRequest(ApiResults.Fail("Quản lý không tồn tại hoặc đã bị vô hiệu hóa."));
            if (!manager.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
                return BadRequest(ApiResults.Fail("Người được chọn phải có vai trò Staff."));

            complex.ComplexName = request.ComplexName.Trim();
            complex.Address = request.Address.Trim();
            complex.ManagerId = request.ManagerId;
            complex.Description = request.Description?.Trim();
            complex.ImageUrl = request.ImageUrl?.Trim();
            complex.Manager = manager;

            await _context.SaveChangesAsync();
            return Ok(ApiResults.Ok(MapToDto(complex), "Cập nhật tổ hợp sân thành công."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var complex = await _context.CourtComplexes
                .Include(c => c.Courts)
                .FirstOrDefaultAsync(c => c.ComplexId == id && !c.IsDeleted);

            if (complex == null)
                return NotFound(ApiResults.Fail("Không tìm thấy tổ hợp sân.", 404));

            if (complex.Courts.Any(c => !c.IsDeleted))
                return BadRequest(ApiResults.Fail("Vui lòng xóa hết sân trước khi xóa tổ hợp."));

            complex.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(ApiResults.Ok(null, "Xóa tổ hợp sân thành công."));
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(ApiResults.Fail("Vui lòng chọn ảnh."));

            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(ApiResults.Fail("Chỉ hỗ trợ ảnh JPG, PNG, WEBP, GIF."));

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "complexes");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using (var stream = System.IO.File.Create(filePath))
                await file.CopyToAsync(stream);

            var url = $"{Request.Scheme}://{Request.Host}/uploads/complexes/{fileName}";
            return Ok(ApiResults.Ok(new ImageUploadResultDto { Url = url }, "Upload ảnh thành công."));
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
