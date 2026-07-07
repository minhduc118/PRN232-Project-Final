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
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Helpers;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/courts")]
    public class CourtsController : ODataController
    {
        private readonly ICourtService _courtService;
        private readonly ICourtTypeRepository _courtTypeRepo;
        private readonly ITimeSlotRepository _timeSlotRepo;
        private readonly AppDbContext _context;

        public CourtsController(ICourtService courtService, ICourtTypeRepository courtTypeRepo, ITimeSlotRepository timeSlotRepo, AppDbContext context)
        {
            _courtService = courtService;
            _courtTypeRepo = courtTypeRepo;
            _timeSlotRepo = timeSlotRepo;
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
                        ImageUrl = c.CourtImages.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(courts);
            }
            else
            {
                var result = await _courtService.SearchCourtsAsync(searchParams);
                return Ok(result);
            }
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
                return NotFound(new { message = "Không tìm thấy sân." });

            var combinedResult = new
            {
                courtId = courtFromDb.CourtId,
                courtName = courtFromDb.CourtName,
                courtCode = courtFromDb.CourtCode,
                courtTypeId = courtFromDb.CourtTypeId,
                courtTypeName = courtFromDb.CourtType.TypeName,
                complexId = courtFromDb.ComplexId,
                complexName = courtFromDb.Complex?.ComplexName,
                description = "", 
                location = courtFromDb.Complex?.Address, 
                capacity = 4, 
                surface = "Acrylic", 
                status = courtFromDb.Status.ToString(),
                openTime = courtFromDb.OpenTime,
                closeTime = courtFromDb.CloseTime,
                pricePerHour = courtFromDb.PricePerHour,
                courtSize = courtFromDb.CourtSize,
                imageUrl = courtFromDb.CourtImages.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() ?? "",
                imageUrls = courtFromDb.CourtImages.Select(i => i.ImageUrl).ToList(),
                createdAt = DateTime.UtcNow,
                
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
                    startTime = p.StartTime,
                    endTime = p.EndTime,
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

            return Ok(combinedResult);
        }


        // GET /api/courts/{id}/availability?date=YYYY-MM-DD
        [HttpGet("{id:int}/availability")]
        public async Task<IActionResult> GetCourtAvailability(int id, [FromQuery] DateTime date)
        {
            var availability = await _courtService.GetCourtAvailabilityAsync(id, date);
            if (availability is null)
                return NotFound(new { message = "Không tìm thấy sân." });

            return Ok(availability);

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
                return BadRequest(new { message = "Tên sân không được để trống." });
            if (string.IsNullOrWhiteSpace(dto.CourtCode))
                return BadRequest(new { message = "Mã sân không được để trống." });

            if (await _courtService.ExistsByCodeAsync(dto.CourtCode))
                return Conflict(new { message = "Mã sân đã tồn tại." });

            if (!TimeSpan.TryParse(dto.OpenTime, out var openTime) || !TimeSpan.TryParse(dto.CloseTime, out var closeTime))
                return BadRequest(new { message = "Thời gian hoạt động không đúng định dạng." });

            if (!System.Enum.TryParse<SportCourtManagent_Server.Enums.CourtStatus>(dto.Status, true, out var status))
                status = SportCourtManagent_Server.Enums.CourtStatus.Available;

            var court = new Court
            {
                CourtName = dto.CourtName.Trim(),
                CourtCode = dto.CourtCode.Trim(),
                CourtTypeId = dto.CourtTypeId,
                ComplexId = dto.ComplexId,
                Status = status,
                OpenTime = openTime,
                CloseTime = closeTime,
                PricePerHour = dto.PricePerHour,
                CourtSize = dto.CourtSize,
                IsDeleted = false
            };

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                var courtImage = new CourtImage
                {
                    CourtId = court.CourtId,
                    ImageUrl = dto.ImageUrl.Trim(),
                    IsPrimary = true
                };
                _context.CourtImages.Add(courtImage);
                await _context.SaveChangesAsync();
            }

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
                ImageUrl = dto.ImageUrl
            };

            return StatusCode(201, result);
        }

        // PUT /api/courts/{id} — Update court (Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CourtDto dto)
        {
            var court = await _courtService.GetByIdAsync(id);
            if (court == null)
                return NotFound(new { message = "Không tìm thấy sân." });

            if (string.IsNullOrWhiteSpace(dto.CourtName))
                return BadRequest(new { message = "Tên sân không được để trống." });
            if (string.IsNullOrWhiteSpace(dto.CourtCode))
                return BadRequest(new { message = "Mã sân không được để trống." });

            if (await _courtService.ExistsByCodeAsync(dto.CourtCode, id))
                return Conflict(new { message = "Mã sân đã tồn tại." });

            try
            {
                await _courtService.UpdateAsync(id, dto);
                return Ok(ApiResults.Ok(null, "Cập nhật sân thể thao thành công."));
            }
            catch (System.Collections.Generic.KeyNotFoundException) { return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404)); }
            catch (System.ArgumentException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
            catch (System.InvalidOperationException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }

        // DELETE /api/courts/{id} — Delete court (Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var court = await _courtService.GetByIdAsync(id);
                if (court == null)
                    return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));

                await _courtService.DeleteAsync(id);
                return Ok(ApiResults.Ok(null, "Xóa sân thành công."));
            }
            catch (System.InvalidOperationException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }
    }
}
