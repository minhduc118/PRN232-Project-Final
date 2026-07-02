using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/courts")]
    public class CourtsController : ControllerBase
    {
        private readonly ICourtService _courtService;
        private readonly AppDbContext _context;

        public CourtsController(ICourtService courtService, AppDbContext context)
        {
            _courtService = courtService;
            _context = context;
        }

        // GET /api/courts — Combined REST Search and Complex courts list
        [HttpGet]
        public async Task<IActionResult> SearchCourts([FromQuery] CourtSearchParams searchParams, [FromQuery] int? complexId)
        {
            if (complexId.HasValue)
            {
                var query = _context.Courts
                    .Include(c => c.CourtType)
                    .Include(c => c.Complex)
                    .Include(c => c.CourtImages)
                    .Where(c => !c.IsDeleted && c.ComplexId == complexId.Value)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchParams.Status) &&
                    System.Enum.TryParse<SportCourtManagent_Server.Enums.CourtStatus>(searchParams.Status, true, out var statusEnum))
                {
                    query = query.Where(c => c.Status == statusEnum);
                }

                var courts = await query
                    .OrderBy(c => c.CourtName)
                    .Select(c => new CourtDto
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
                    })
                    .ToListAsync();

                return Ok(ApiResults.Ok(courts, "Lấy danh sách sân thành công."));
            }
            else
            {
                var result = await _courtService.SearchCourtsAsync(searchParams);
                return Ok(ApiResults.Ok(result, "Tìm kiếm sân thành công."));
            }
        }

        // GET /api/courts/{id} — Court detail (Combined compatibility)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCourtDetail(int id)
        {
            var customerDetail = await _courtService.GetCourtDetailAsync(id);
            
            var courtFromDb = await _context.Courts
                .Include(c => c.Complex)
                .Include(c => c.CourtType)
                .Include(c => c.CourtImages)
                .FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);

            if (courtFromDb == null)
                return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));

            var combinedResult = new
            {
                courtId = courtFromDb.CourtId,
                courtName = courtFromDb.CourtName,
                courtCode = courtFromDb.CourtCode,
                courtTypeId = courtFromDb.CourtTypeId,
                courtTypeName = courtFromDb.CourtType.TypeName,
                complexId = courtFromDb.ComplexId,
                complexName = courtFromDb.Complex?.ComplexName,
                description = courtFromDb.Description,
                location = courtFromDb.Location,
                capacity = courtFromDb.Capacity,
                surface = courtFromDb.Surface,
                status = courtFromDb.Status.ToString(),
                openTime = courtFromDb.OpenTime.ToString(@"hh\:mm"),
                closeTime = courtFromDb.CloseTime.ToString(@"hh\:mm"),
                pricePerHour = courtFromDb.PricePerHour,
                courtSize = courtFromDb.CourtSize,
                imageUrl = courtFromDb.CourtImages.OrderBy(i => i.CourtImageId).Select(i => i.ImageUrl).FirstOrDefault() ?? courtFromDb.ImageUrl,
                imageUrls = courtFromDb.CourtImages.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
                createdAt = courtFromDb.CreatedAt,
                
                courtType = customerDetail != null ? new {
                    courtTypeId = customerDetail.CourtType.CourtTypeId,
                    typeName = customerDetail.CourtType.TypeName,
                    isActive = customerDetail.CourtType.IsActive
                } : null,
                images = customerDetail != null ? customerDetail.Images.Select(img => new {
                    imageId = img.CourtImageId,
                    imageUrl = img.ImageUrl,
                    isPrimary = img.IsPrimary,
                    sortOrder = 1
                }).ToList() : null,
                pricings = customerDetail != null ? customerDetail.Pricings.Select(p => new {
                    pricingId = p.PricingId,
                    slotId = p.SlotId,
                    slotName = p.SlotName,
                    startTime = p.StartTime.ToString(@"hh\:mm"),
                    endTime = p.EndTime.ToString(@"hh\:mm"),
                    dayType = p.DayType,
                    price = p.Price,
                    peakMultiplier = 1.0
                }).ToList() : null,
                reviewSummary = customerDetail != null ? new {
                    averageRating = customerDetail.ReviewSummary.AverageRating,
                    totalReviews = customerDetail.ReviewSummary.TotalReviews,
                    ratingDistribution = customerDetail.ReviewSummary.RatingDistribution
                } : null
            };

            return Ok(ApiResults.Ok(combinedResult, "Lấy thông tin chi tiết sân thành công."));
        }

        // GET /api/courts/{id}/availability?date=YYYY-MM-DD
        [HttpGet("{id:int}/availability")]
        public async Task<IActionResult> GetCourtAvailability(int id, [FromQuery] DateTime date)
        {
            var availability = await _courtService.GetCourtAvailabilityAsync(id, date);
            if (availability is null)
                return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));

            return Ok(ApiResults.Ok(availability, "Lấy danh sách khe giờ trống thành công."));
        }

        // GET /odata/courts — OData query
        [HttpGet("/odata/courts")]
        [EnableQuery(MaxTop = 100, PageSize = 50)]
        public IActionResult GetOData()
        {
            return Ok(_courtService.GetCourtsODataQueryable());
        }

        // POST /api/courts — Create court (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CourtDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CourtName))
                return BadRequest(ApiResults.Fail("Tên sân không được để trống."));
            if (string.IsNullOrWhiteSpace(dto.CourtCode))
                return BadRequest(ApiResults.Fail("Mã sân không được để trống."));

            if (await _context.Courts.AnyAsync(c => c.CourtCode == dto.CourtCode && !c.IsDeleted))
                return Conflict(ApiResults.Fail("Mã sân đã tồn tại.", 409));

            if (!TimeSpan.TryParse(dto.OpenTime, out var openTime) || !TimeSpan.TryParse(dto.CloseTime, out var closeTime))
                return BadRequest(ApiResults.Fail("Thời gian hoạt động không đúng định dạng."));

            if (!System.Enum.TryParse<SportCourtManagent_Server.Enums.CourtStatus>(dto.Status, true, out var status))
                status = SportCourtManagent_Server.Enums.CourtStatus.Available;

            var court = new Court
            {
                CourtName = dto.CourtName.Trim(),
                CourtCode = dto.CourtCode.Trim(),
                CourtTypeId = dto.CourtTypeId,
                ComplexId = dto.ComplexId,
                Description = dto.Description?.Trim() ?? string.Empty,
                Location = dto.Location?.Trim() ?? string.Empty,
                Capacity = 4,
                Status = status,
                OpenTime = openTime,
                CloseTime = closeTime,
                PricePerHour = dto.PricePerHour,
                CourtSize = dto.CourtSize,
                CreatedAt = DateTime.UtcNow
            };

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            await _context.Entry(court).Reference(c => c.CourtType).LoadAsync();
            await _context.Entry(court).Reference(c => c.Complex).LoadAsync();

            var result = new CourtDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                CourtCode = court.CourtCode,
                CourtTypeId = court.CourtTypeId,
                CourtTypeName = court.CourtType.TypeName,
                ComplexId = court.ComplexId,
                ComplexName = court.Complex?.ComplexName,
                Status = court.Status.ToString(),
                OpenTime = court.OpenTime.ToString(@"hh\:mm"),
                CloseTime = court.CloseTime.ToString(@"hh\:mm"),
                PricePerHour = court.PricePerHour,
                CourtSize = court.CourtSize,
                ImageUrl = court.ImageUrl
            };

            return StatusCode(201, ApiResults.Ok(result, "Tạo sân thành công.", 201));
        }

        // PUT /api/courts/{id} — Update court (Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CourtDto dto)
        {
            var court = await _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);

            if (court == null)
                return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));

            if (string.IsNullOrWhiteSpace(dto.CourtName))
                return BadRequest(ApiResults.Fail("Tên sân không được để trống."));
            if (string.IsNullOrWhiteSpace(dto.CourtCode))
                return BadRequest(ApiResults.Fail("Mã sân không được để trống."));

            if (await _context.Courts.AnyAsync(c => c.CourtCode == dto.CourtCode && c.CourtId != id && !c.IsDeleted))
                return Conflict(ApiResults.Fail("Mã sân đã tồn tại.", 409));

            if (!TimeSpan.TryParse(dto.OpenTime, out var openTime) || !TimeSpan.TryParse(dto.CloseTime, out var closeTime))
                return BadRequest(ApiResults.Fail("Thời gian hoạt động không đúng định dạng."));

            if (!System.Enum.TryParse<SportCourtManagent_Server.Enums.CourtStatus>(dto.Status, true, out var status))
                status = SportCourtManagent_Server.Enums.CourtStatus.Available;

            court.CourtName = dto.CourtName.Trim();
            court.CourtCode = dto.CourtCode.Trim();
            court.CourtTypeId = dto.CourtTypeId;
            court.ComplexId = dto.ComplexId;
            court.Description = dto.Description?.Trim() ?? string.Empty;
            court.Location = dto.Location?.Trim() ?? string.Empty;
            court.Status = status;
            court.OpenTime = openTime;
            court.CloseTime = closeTime;
            court.PricePerHour = dto.PricePerHour;
            court.CourtSize = dto.CourtSize;

            await _context.SaveChangesAsync();

            var result = new CourtDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                CourtCode = court.CourtCode,
                CourtTypeId = court.CourtTypeId,
                CourtTypeName = court.CourtType.TypeName,
                ComplexId = court.ComplexId,
                ComplexName = court.Complex?.ComplexName,
                Status = court.Status.ToString(),
                OpenTime = court.OpenTime.ToString(@"hh\:mm"),
                CloseTime = court.CloseTime.ToString(@"hh\:mm"),
                PricePerHour = court.PricePerHour,
                CourtSize = court.CourtSize,
                ImageUrl = court.ImageUrl
            };

            return Ok(ApiResults.Ok(result, "Cập nhật sân thành công."));
        }

        // DELETE /api/courts/{id} — Delete court (Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var court = await _context.Courts.FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);
            if (court == null)
                return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));

            court.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResults.Ok(null, "Xóa sân thành công."));
        }
    }
}
