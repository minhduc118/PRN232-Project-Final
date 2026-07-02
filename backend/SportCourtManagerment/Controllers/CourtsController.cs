<<<<<<< HEAD
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Courts;
using SportCourtManagerment.DTOs.Reviews;
using SportCourtManagerment.Services.Interfaces;

namespace SportCourtManagerment.Controllers;

/// <summary>
/// REST and OData endpoints for court search, detail, availability, and reviews.
/// </summary>
=======
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Courts;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.Controllers;

>>>>>>> 596195fe62bf1538b025893c422406ab017e3e0a
[ApiController]
[Route("api/courts")]
public class CourtsController : ControllerBase
{
<<<<<<< HEAD
  private readonly ICourtService  _courtService;
  private readonly IReviewService _reviewService;

  public CourtsController(ICourtService courtService, IReviewService reviewService)
  {
    _courtService  = courtService;
    _reviewService = reviewService;
  }

  
  //  GET /api/courts — Search with filters + pagination (REST)
  
  [HttpGet]
  public async Task<IActionResult> SearchCourts([FromQuery] CourtSearchParams searchParams)
  {
    var result = await _courtService.SearchCourtsAsync(searchParams);
    return Ok(ApiResponse<PagedResult<CourtListDto>>.Ok(result,
      "Tìm kiếm sân thành công."));
  }

 
  //  GET /api/courts/{id} — Court detail
  
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetCourtDetail(int id)
  {
    var detail = await _courtService.GetCourtDetailAsync(id);
    if (detail is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân.", 404));

    return Ok(ApiResponse<CourtDetailDto>.Ok(detail,
      "Lấy chi tiết sân thành công."));
  }

  
  //  GET /api/courts/{id}/availability?date=YYYY-MM-DD
  
  [HttpGet("{id:int}/availability")]
  public async Task<IActionResult> GetCourtAvailability(
    int id, [FromQuery] DateOnly date)
  {
    var availability = await _courtService.GetCourtAvailabilityAsync(id, date);
    if (availability is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân.", 404));

    return Ok(ApiResponse<CourtAvailabilityDto>.Ok(availability,
      "Lấy lịch trống thành công."));
  }


  //  GET /api/courts/{courtId}/reviews — Court reviews with pagination
  
  [HttpGet("{courtId:int}/reviews")]
  public async Task<IActionResult> GetCourtReviews(
    int courtId,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
  {
    var result = await _reviewService.GetCourtReviewsAsync(courtId, pageNumber, pageSize);
    return Ok(ApiResponse<PagedResult<ReviewDto>>.Ok(result,
      "Lấy đánh giá sân thành công."));
  }


  //  POST /api/courts/{courtId}/reviews — Create a new review (requires authentication)
  
  /// <summary>
  /// Creates a new review for a completed booking on this court.
  /// Requires JWT authentication. UserId is extracted from the token.
  /// Each booking can only have one review (1:1 relationship).
  /// </summary>
  [HttpPost("{courtId:int}/reviews")]
  [Authorize]
  public async Task<IActionResult> CreateReview(
    int courtId,
    [FromBody] CreateReviewDto dto)
  {
    // Extract userId from JWT claims
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                  ?? User.FindFirst("sub");

    if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
      return Unauthorized(ApiResponse<object>.Fail("Token không hợp lệ.", 401));

    var (review, error) = await _reviewService.CreateReviewAsync(courtId, userId, dto);

    if (error is not null)
      return BadRequest(ApiResponse<object>.Fail(error));

    return StatusCode(201,
      ApiResponse<ReviewDto>.Created(review, "Tạo đánh giá thành công."));
  }
}

=======
  private readonly ApplicationDbContext _db;

  public CourtsController(ApplicationDbContext db)
  {
    _db = db;
  }

  // ─────────────────────────────────────────────
  // GET /api/courts
  // ─────────────────────────────────────────────
  [HttpGet]
  public async Task<IActionResult> GetAll([FromQuery] int? complexId, [FromQuery] int? courtTypeId, [FromQuery] string? status)
  {
    var query = _db.Courts
      .Include(c => c.CourtType)
      .Include(c => c.Complex)
      .Include(c => c.CourtImages)
      .Where(c => !c.IsDeleted);

    if (complexId.HasValue)
    {
      query = query.Where(c => c.ComplexId == complexId.Value);
    }

    if (courtTypeId.HasValue)
    {
      query = query.Where(c => c.CourtTypeId == courtTypeId.Value);
    }

    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CourtStatus>(status, true, out var filterStatus))
    {
      query = query.Where(c => c.Status == filterStatus);
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
        ComplexName = c.Complex != null ? c.Complex.ComplexName : null,
        Description = c.Description,
        Location = c.Location,
        Capacity = c.Capacity,
        Surface = c.Surface,
        ImageUrl = c.ImageUrl,
        Status = c.Status.ToString(),
        OpenTime = c.OpenTime.ToString("HH:mm"),
        CloseTime = c.CloseTime.ToString("HH:mm"),
        PricePerHour = c.PricePerHour,
        CourtSize = c.CourtSize,
        ImageUrls = c.CourtImages.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).ToList(),
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
      })
      .ToListAsync();

    return Ok(ApiResponse<List<CourtDto>>.Ok(courts, "Lấy danh sách sân thành công."));
  }

  // ─────────────────────────────────────────────
  // GET /api/courts/{id}
  // ─────────────────────────────────────────────
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetById(int id)
  {
    var c = await _db.Courts
      .Include(c => c.CourtType)
      .Include(c => c.Complex)
      .Include(c => c.CourtImages)
      .FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);

    if (c is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân thể thao hoặc đã bị xóa.", 404));

    var dto = new CourtDto
    {
      CourtId = c.CourtId,
      CourtName = c.CourtName,
      CourtCode = c.CourtCode,
      CourtTypeId = c.CourtTypeId,
      CourtTypeName = c.CourtType.TypeName,
      ComplexId = c.ComplexId,
      ComplexName = c.Complex != null ? c.Complex.ComplexName : null,
      Description = c.Description,
      Location = c.Location,
      Capacity = c.Capacity,
      Surface = c.Surface,
      ImageUrl = c.ImageUrl,
      Status = c.Status.ToString(),
      OpenTime = c.OpenTime.ToString("HH:mm"),
      CloseTime = c.CloseTime.ToString("HH:mm"),
      PricePerHour = c.PricePerHour,
      CourtSize = c.CourtSize,
      ImageUrls = c.CourtImages.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).ToList(),
      CreatedAt = c.CreatedAt,
      UpdatedAt = c.UpdatedAt
    };

    return Ok(ApiResponse<CourtDto>.Ok(dto, "Lấy thông tin chi tiết sân thành công."));
  }

  // ─────────────────────────────────────────────
  // POST /api/courts
  // ─────────────────────────────────────────────
  [Authorize]
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateCourtDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ."));

    // Check unique CourtCode
    if (await _db.Courts.AnyAsync(c => c.CourtCode == dto.CourtCode && !c.IsDeleted))
    {
      return Conflict(ApiResponse<object>.Fail("Mã sân đã tồn tại trong hệ thống. Vui lòng chọn mã khác.", 409));
    }

    if (!TimeOnly.TryParse(dto.OpenTime, out var openTime) || !TimeOnly.TryParse(dto.CloseTime, out var closeTime))
    {
      return BadRequest(ApiResponse<object>.Fail("Giờ mở/đóng cửa không đúng định dạng HH:mm."));
    }

    if (!Enum.TryParse<CourtStatus>(dto.Status, true, out var status))
    {
      status = CourtStatus.Available;
    }

    var court = new Court
    {
      CourtName = dto.CourtName,
      CourtCode = dto.CourtCode,
      CourtTypeId = dto.CourtTypeId,
      ComplexId = dto.ComplexId,
      Description = dto.Description,
      Location = dto.Location,
      Capacity = dto.Capacity,
      Surface = dto.Surface,
      ImageUrl = dto.ImageUrl,
      Status = status,
      OpenTime = openTime,
      CloseTime = closeTime,
      PricePerHour = dto.PricePerHour,
      CourtSize = dto.CourtSize,
      CreatedAt = DateTime.UtcNow
    };

    _db.Courts.Add(court);
    await _db.SaveChangesAsync();

    // Sync gallery images
    if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
    {
      var sortOrder = 1;
      foreach (var url in dto.ImageUrls)
      {
        if (!string.IsNullOrWhiteSpace(url))
        {
          _db.CourtImages.Add(new CourtImage
          {
            CourtId = court.CourtId,
            ImageUrl = url.Trim(),
            IsPrimary = false,
            SortOrder = sortOrder++,
            CreatedAt = DateTime.UtcNow
          });
        }
      }
      await _db.SaveChangesAsync();
    }

    // Load type and complex name for returning DTO
    var loadedCourt = await _db.Courts
      .Include(c => c.CourtType)
      .Include(c => c.Complex)
      .Include(c => c.CourtImages)
      .FirstAsync(c => c.CourtId == court.CourtId);

    var result = new CourtDto
    {
      CourtId = loadedCourt.CourtId,
      CourtName = loadedCourt.CourtName,
      CourtCode = loadedCourt.CourtCode,
      CourtTypeId = loadedCourt.CourtTypeId,
      CourtTypeName = loadedCourt.CourtType.TypeName,
      ComplexId = loadedCourt.ComplexId,
      ComplexName = loadedCourt.Complex != null ? loadedCourt.Complex.ComplexName : null,
      Description = loadedCourt.Description,
      Location = loadedCourt.Location,
      Capacity = loadedCourt.Capacity,
      Surface = loadedCourt.Surface,
      ImageUrl = loadedCourt.ImageUrl,
      Status = loadedCourt.Status.ToString(),
      OpenTime = loadedCourt.OpenTime.ToString("HH:mm"),
      CloseTime = loadedCourt.CloseTime.ToString("HH:mm"),
      PricePerHour = loadedCourt.PricePerHour,
      CourtSize = loadedCourt.CourtSize,
      ImageUrls = loadedCourt.CourtImages.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).ToList(),
      CreatedAt = loadedCourt.CreatedAt
    };

    return CreatedAtAction(nameof(GetById), new { id = court.CourtId }, ApiResponse<CourtDto>.Created(result, "Tạo sân thể thao mới thành công."));
  }

  // ─────────────────────────────────────────────
  // PUT /api/courts/{id}
  // ─────────────────────────────────────────────
  [Authorize]
  [HttpPut("{id}")]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateCourtDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ."));

    var court = await _db.Courts
      .Include(c => c.CourtImages)
      .FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);

    if (court is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân thể thao cần cập nhật.", 404));

    // Check unique CourtCode
    if (await _db.Courts.AnyAsync(c => c.CourtCode == dto.CourtCode && c.CourtId != id && !c.IsDeleted))
    {
      return Conflict(ApiResponse<object>.Fail("Mã sân đã tồn tại trong hệ thống. Vui lòng chọn mã khác.", 409));
    }

    if (!TimeOnly.TryParse(dto.OpenTime, out var openTime) || !TimeOnly.TryParse(dto.CloseTime, out var closeTime))
    {
      return BadRequest(ApiResponse<object>.Fail("Giờ mở/đóng cửa không đúng định dạng HH:mm."));
    }

    if (!Enum.TryParse<CourtStatus>(dto.Status, true, out var status))
    {
      status = CourtStatus.Available;
    }

    court.CourtName = dto.CourtName;
    court.CourtCode = dto.CourtCode;
    court.CourtTypeId = dto.CourtTypeId;
    court.ComplexId = dto.ComplexId;
    court.Description = dto.Description;
    court.Location = dto.Location;
    court.Capacity = dto.Capacity;
    court.Surface = dto.Surface;
    court.ImageUrl = dto.ImageUrl;
    court.Status = status;
    court.OpenTime = openTime;
    court.CloseTime = closeTime;
    court.PricePerHour = dto.PricePerHour;
    court.CourtSize = dto.CourtSize;
    court.UpdatedAt = DateTime.UtcNow;

    // Sync gallery images (delete old and recreate)
    _db.CourtImages.RemoveRange(court.CourtImages);
    
    if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
    {
      var sortOrder = 1;
      foreach (var url in dto.ImageUrls)
      {
        if (!string.IsNullOrWhiteSpace(url))
        {
          _db.CourtImages.Add(new CourtImage
          {
            CourtId = court.CourtId,
            ImageUrl = url.Trim(),
            IsPrimary = false,
            SortOrder = sortOrder++,
            CreatedAt = DateTime.UtcNow
          });
        }
      }
    }

    await _db.SaveChangesAsync();

    var loadedCourt = await _db.Courts
      .Include(c => c.CourtType)
      .Include(c => c.Complex)
      .Include(c => c.CourtImages)
      .FirstAsync(c => c.CourtId == court.CourtId);

    var updatedDto = new CourtDto
    {
      CourtId = loadedCourt.CourtId,
      CourtName = loadedCourt.CourtName,
      CourtCode = loadedCourt.CourtCode,
      CourtTypeId = loadedCourt.CourtTypeId,
      CourtTypeName = loadedCourt.CourtType.TypeName,
      ComplexId = loadedCourt.ComplexId,
      ComplexName = loadedCourt.Complex != null ? loadedCourt.Complex.ComplexName : null,
      Description = loadedCourt.Description,
      Location = loadedCourt.Location,
      Capacity = loadedCourt.Capacity,
      Surface = loadedCourt.Surface,
      ImageUrl = loadedCourt.ImageUrl,
      Status = loadedCourt.Status.ToString(),
      OpenTime = loadedCourt.OpenTime.ToString("HH:mm"),
      CloseTime = loadedCourt.CloseTime.ToString("HH:mm"),
      PricePerHour = loadedCourt.PricePerHour,
      CourtSize = loadedCourt.CourtSize,
      ImageUrls = loadedCourt.CourtImages.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).ToList(),
      CreatedAt = loadedCourt.CreatedAt,
      UpdatedAt = loadedCourt.UpdatedAt
    };

    return Ok(ApiResponse<CourtDto>.Ok(updatedDto, "Cập nhật sân thể thao thành công."));
  }

  // ─────────────────────────────────────────────
  // DELETE /api/courts/{id}
  // ─────────────────────────────────────────────
  [Authorize]
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var court = await _db.Courts.FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);
    if (court is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy sân thể thao cần xóa.", 404));

    // Soft delete
    court.IsDeleted = true;
    court.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    return Ok(ApiResponse<object>.Ok(null, "Xóa sân thể thao thành công."));
  }

  // ─────────────────────────────────────────────
  // GET /api/court-types (Bypasses prefix)
  // ─────────────────────────────────────────────
  [HttpGet("~/api/court-types")]
  public async Task<IActionResult> GetCourtTypes()
  {
    var types = await _db.CourtTypes
      .Where(t => t.IsActive)
      .Select(t => new CourtTypeDto
      {
        CourtTypeId = t.CourtTypeId,
        TypeName = t.TypeName,
        IsActive = t.IsActive
      })
      .ToListAsync();

    return Ok(ApiResponse<List<CourtTypeDto>>.Ok(types, "Lấy danh sách loại sân thành công."));
  }
}
>>>>>>> 596195fe62bf1538b025893c422406ab017e3e0a
