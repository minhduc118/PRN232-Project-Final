using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Courts;
using SportCourtManagerment.Models;
using SportCourtManagerment.Services;

namespace SportCourtManagerment.Controllers;

[ApiController]
[Route("api/complexes")]
public class CourtComplexesController : ControllerBase
{
  private readonly ApplicationDbContext _db;
  private readonly CloudinaryService _cloudinary;

  public CourtComplexesController(ApplicationDbContext db, CloudinaryService cloudinary)
  {
    _db = db;
    _cloudinary = cloudinary;
  }

  // ─────────────────────────────────────────────
  // GET /api/complexes?search=&courtTypeId=&page=&pageSize=
  // Unified: search + filter + pagination in ONE call
  // ─────────────────────────────────────────────
  [HttpGet]
  public async Task<IActionResult> GetAll(
    [FromQuery] string? search      = null,
    [FromQuery] int?    courtTypeId = null,
    [FromQuery] int     page        = 1,
    [FromQuery] int     pageSize    = 8)
  {
    pageSize = Math.Clamp(pageSize, 1, 100);
    page     = Math.Max(1, page);

    // Include Manager for dynamic phone/name resolution
    var query = _db.CourtComplexes
      .Include(cx => cx.Courts)
      .Include(cx => cx.Manager)
      .Where(cx => !cx.IsDeleted)
      .AsQueryable();

    // Full-text search across name / address / manager name/phone
    if (!string.IsNullOrWhiteSpace(search))
    {
      var term = search.Trim();
      query = query.Where(cx =>
        cx.ComplexName.Contains(term) ||
        cx.Address.Contains(term) ||
        (cx.Manager != null && cx.Manager.FullName.Contains(term)) ||
        (cx.Manager != null && cx.Manager.Phone != null && cx.Manager.Phone.Contains(term)));
    }

    // Filter by court type
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
      .Select(cx => new CourtComplexDto
      {
        ComplexId         = cx.ComplexId,
        ComplexName       = cx.ComplexName,
        Address           = cx.Address,
        // Derived from Manager navigation property
        Phone             = cx.Manager != null ? cx.Manager.Phone : null,
        ManagerName       = cx.Manager != null ? cx.Manager.FullName : null,
        ManagerId         = cx.ManagerId,
        Description       = cx.Description,
        ImageUrl          = cx.ImageUrl,
        TotalCourts       = cx.Courts.Count(c => !c.IsDeleted),
        ActiveCourts      = cx.Courts.Count(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Available),
        MaintenanceCourts = cx.Courts.Count(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Maintenance),
        InactiveCourts    = cx.Courts.Count(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Inactive),
        CourtTypeIds      = cx.Courts.Where(c => !c.IsDeleted).Select(c => c.CourtTypeId).Distinct().ToList(),
        CreatedAt         = cx.CreatedAt
      })
      .ToListAsync();

    var totalComplexes    = await _db.CourtComplexes.CountAsync(cx => !cx.IsDeleted);
    var totalCourts       = await _db.Courts.CountAsync(c => !c.IsDeleted);
    var activeCourts      = await _db.Courts.CountAsync(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Available);
    var maintenanceCourts = await _db.Courts.CountAsync(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Maintenance);
    var inactiveCourts    = await _db.Courts.CountAsync(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Inactive);

    var stats = new ComplexStatsDto
    {
      TotalComplexes    = totalComplexes,
      TotalCourts       = totalCourts,
      ActiveCourts      = activeCourts,
      MaintenanceCourts = maintenanceCourts,
      InactiveCourts    = inactiveCourts
    };

    var result = new PagedComplexResult
    {
      Items      = items,
      TotalCount = totalCount,
      Page       = page,
      PageSize   = pageSize,
      TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
      Stats      = stats
    };

    return Ok(ApiResponse<PagedComplexResult>.Ok(result, "Lấy danh sách tổ hợp sân thành công."));
  }

  // ─────────────────────────────────────────────
  // GET /api/complexes/stats
  // ─────────────────────────────────────────────
  [HttpGet("stats")]
  public async Task<IActionResult> GetStats()
  {
    var totalComplexes    = await _db.CourtComplexes.CountAsync(cx => !cx.IsDeleted);
    var totalCourts       = await _db.Courts.CountAsync(c => !c.IsDeleted);
    var activeCourts      = await _db.Courts.CountAsync(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Available);
    var maintenanceCourts = await _db.Courts.CountAsync(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Maintenance);
    var inactiveCourts    = await _db.Courts.CountAsync(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Inactive);

    var stats = new ComplexStatsDto
    {
      TotalComplexes    = totalComplexes,
      TotalCourts       = totalCourts,
      ActiveCourts      = activeCourts,
      MaintenanceCourts = maintenanceCourts,
      InactiveCourts    = inactiveCourts
    };

    return Ok(ApiResponse<ComplexStatsDto>.Ok(stats, "Lấy thống kê hệ thống thành công."));
  }

  // ─────────────────────────────────────────────
  // GET /api/complexes/{id}
  // ─────────────────────────────────────────────
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetById(int id)
  {
    var cx = await _db.CourtComplexes
      .Include(c => c.Courts)
      .Include(c => c.Manager)
      .FirstOrDefaultAsync(c => c.ComplexId == id && !c.IsDeleted);

    if (cx is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy tổ hợp sân hoặc đã bị xóa.", 404));

    var dto = new CourtComplexDto
    {
      ComplexId         = cx.ComplexId,
      ComplexName       = cx.ComplexName,
      Address           = cx.Address,
      Phone             = cx.Manager?.Phone,
      ManagerName       = cx.Manager?.FullName,
      ManagerId         = cx.ManagerId,
      Description       = cx.Description,
      ImageUrl          = cx.ImageUrl,
      TotalCourts       = cx.Courts.Count(c => !c.IsDeleted),
      ActiveCourts      = cx.Courts.Count(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Available),
      MaintenanceCourts = cx.Courts.Count(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Maintenance),
      InactiveCourts    = cx.Courts.Count(c => !c.IsDeleted && c.Status == Enums.CourtStatus.Inactive),
      CourtTypeIds      = cx.Courts.Where(c => !c.IsDeleted).Select(c => c.CourtTypeId).Distinct().ToList(),
      CreatedAt         = cx.CreatedAt
    };

    return Ok(ApiResponse<CourtComplexDto>.Ok(dto, "Lấy thông tin tổ hợp sân thành công."));
  }

  // ─────────────────────────────────────────────
  // POST /api/complexes
  // ─────────────────────────────────────────────
  [Authorize]
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateComplexDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ."));

    // Validate ManagerId exists if provided
    if (dto.ManagerId.HasValue)
    {
      var managerExists = await _db.Users.AnyAsync(u => u.UserId == dto.ManagerId.Value && u.IsActive);
      if (!managerExists)
        return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy quản lý với mã #{dto.ManagerId.Value}."));
    }

    var cx = new CourtComplex
    {
      ComplexName = dto.ComplexName,
      Address     = dto.Address,
      ManagerId   = dto.ManagerId,
      Description = dto.Description,
      ImageUrl    = dto.ImageUrl,
      CreatedAt   = DateTime.UtcNow
    };

    _db.CourtComplexes.Add(cx);
    await _db.SaveChangesAsync();

    // Reload with Manager for response DTO
    await _db.Entry(cx).Reference(c => c.Manager).LoadAsync();

    var result = new CourtComplexDto
    {
      ComplexId         = cx.ComplexId,
      ComplexName       = cx.ComplexName,
      Address           = cx.Address,
      Phone             = cx.Manager?.Phone,
      ManagerName       = cx.Manager?.FullName,
      ManagerId         = cx.ManagerId,
      Description       = cx.Description,
      ImageUrl          = cx.ImageUrl,
      TotalCourts       = 0,
      ActiveCourts      = 0,
      MaintenanceCourts = 0,
      InactiveCourts    = 0,
      CreatedAt         = cx.CreatedAt
    };

    return CreatedAtAction(nameof(GetById), new { id = cx.ComplexId }, ApiResponse<CourtComplexDto>.Created(result, "Tạo tổ hợp sân mới thành công."));
  }

  // ─────────────────────────────────────────────
  // PUT /api/complexes/{id}
  // ─────────────────────────────────────────────
  [Authorize]
  [HttpPut("{id}")]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateComplexDto dto)
  {
    if (!ModelState.IsValid)
      return BadRequest(ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ."));

    var cx = await _db.CourtComplexes
      .Include(c => c.Manager)
      .FirstOrDefaultAsync(c => c.ComplexId == id && !c.IsDeleted);
    if (cx is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy tổ hợp sân cần cập nhật.", 404));

    // Validate ManagerId exists if provided
    if (dto.ManagerId.HasValue)
    {
      var managerExists = await _db.Users.AnyAsync(u => u.UserId == dto.ManagerId.Value && u.IsActive);
      if (!managerExists)
        return BadRequest(ApiResponse<object>.Fail($"Không tìm thấy quản lý với mã #{dto.ManagerId.Value}."));
    }

    cx.ComplexName = dto.ComplexName;
    cx.Address     = dto.Address;
    cx.ManagerId   = dto.ManagerId;
    cx.Description = dto.Description;
    cx.ImageUrl    = dto.ImageUrl;
    cx.UpdatedAt   = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    // Reload Manager after potential change of ManagerId
    await _db.Entry(cx).Reference(c => c.Manager).LoadAsync();

    var updatedDto = new CourtComplexDto
    {
      ComplexId         = cx.ComplexId,
      ComplexName       = cx.ComplexName,
      Address           = cx.Address,
      Phone             = cx.Manager?.Phone,
      ManagerName       = cx.Manager?.FullName,
      ManagerId         = cx.ManagerId,
      Description       = cx.Description,
      ImageUrl          = cx.ImageUrl,
      TotalCourts       = await _db.Courts.CountAsync(c => c.ComplexId == cx.ComplexId && !c.IsDeleted),
      ActiveCourts      = await _db.Courts.CountAsync(c => c.ComplexId == cx.ComplexId && !c.IsDeleted && c.Status == Enums.CourtStatus.Available),
      MaintenanceCourts = await _db.Courts.CountAsync(c => c.ComplexId == cx.ComplexId && !c.IsDeleted && c.Status == Enums.CourtStatus.Maintenance),
      InactiveCourts    = await _db.Courts.CountAsync(c => c.ComplexId == cx.ComplexId && !c.IsDeleted && c.Status == Enums.CourtStatus.Inactive),
      CreatedAt         = cx.CreatedAt
    };

    return Ok(ApiResponse<CourtComplexDto>.Ok(updatedDto, "Cập nhật tổ hợp sân thành công."));
  }

  // ─────────────────────────────────────────────
  // DELETE /api/complexes/{id}
  // ─────────────────────────────────────────────
  [Authorize]
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var cx = await _db.CourtComplexes
      .Include(c => c.Courts)
      .FirstOrDefaultAsync(c => c.ComplexId == id && !c.IsDeleted);

    if (cx is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy tổ hợp sân cần xóa.", 404));

    var hasActiveCourts = cx.Courts.Any(c => !c.IsDeleted);
    if (hasActiveCourts)
    {
      return BadRequest(ApiResponse<object>.Fail(
        "Không thể xóa tổ hợp sân khi đang có các sân hoạt động bên trong. Vui lòng di chuyển hoặc xóa hết các sân con trước.", 400));
    }

    cx.IsDeleted = true;
    cx.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    return Ok(ApiResponse<object>.Ok(null, "Xóa tổ hợp sân thành công."));
  }

  // ─────────────────────────────────────────────
  // POST /api/complexes/upload-image
  // Upload ảnh tổ hợp sân lên Cloudinary
  // ─────────────────────────────────────────────
  [Authorize]
  [HttpPost("upload-image")]
  public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
  {
    if (file == null || file.Length == 0)
      return BadRequest(ApiResponse<object>.Fail("File ảnh không được để trống."));

    try
    {
      var url = await _cloudinary.UploadImageAsync(file);
      return Ok(ApiResponse<ImageUploadResultDto>.Ok(
        new ImageUploadResultDto { Url = url },
        "Upload ảnh thành công."));
    }
    catch (Exception ex)
    {
      return StatusCode(500, ApiResponse<object>.Fail($"Upload ảnh thất bại: {ex.Message}"));
    }
  }

  // ─────────────────────────────────────────────
  // GET /api/complexes/{id}/bookings
  // ─────────────────────────────────────────────
  [HttpGet("{id:int}/bookings")]
  public async Task<IActionResult> GetBookings(
    int id,
    [FromQuery] int?    courtId  = null,
    [FromQuery] string? status   = null,
    [FromQuery] string? dateFrom = null,
    [FromQuery] string? dateTo   = null)
  {
    var exists = await _db.CourtComplexes.AnyAsync(cx => cx.ComplexId == id && !cx.IsDeleted);
    if (!exists)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy tổ hợp sân hoặc đã bị xóa.", 404));

    var courtIds = await _db.Courts
      .Where(c => c.ComplexId == id && !c.IsDeleted)
      .Select(c => c.CourtId)
      .ToListAsync();

    var query = _db.Bookings
      .Include(b => b.User)
      .Include(b => b.Court)
      .Include(b => b.Payments)
      .Where(b => courtIds.Contains(b.CourtId))
      .AsQueryable();

    if (courtId.HasValue)
      query = query.Where(b => b.CourtId == courtId.Value);

    if (!string.IsNullOrWhiteSpace(status) &&
        Enum.TryParse<Enums.BookingStatus>(status, true, out var filterStatus))
      query = query.Where(b => b.Status == filterStatus);

    if (DateOnly.TryParse(dateFrom, out var fromDate))
      query = query.Where(b => b.BookingDate >= fromDate);

    if (DateOnly.TryParse(dateTo, out var toDate))
      query = query.Where(b => b.BookingDate <= toDate);

    var bookings = await query
      .OrderByDescending(b => b.BookingDate)
      .ThenByDescending(b => b.StartTime)
      .Select(b => new BookingSummaryDto
      {
        BookingId     = b.BookingId,
        BookingCode   = b.BookingCode,
        UserId        = b.UserId,
        CustomerName  = b.User.FullName,
        CustomerPhone = b.User.Phone,
        CourtId       = b.CourtId,
        CourtName     = b.Court.CourtName,
        BookingDate   = b.BookingDate.ToString("yyyy-MM-dd"),
        StartTime     = b.StartTime.ToString("HH:mm"),
        EndTime       = b.EndTime.ToString("HH:mm"),
        TotalAmount   = b.TotalAmount,
        Status        = b.Status.ToString(),
        PaymentMethod = b.Payments.OrderByDescending(p => p.CreatedAt).Select(p => p.PaymentMethod.ToString()).FirstOrDefault(),
        PaymentStatus = b.Payments.OrderByDescending(p => p.CreatedAt).Select(p => p.Status.ToString()).FirstOrDefault(),
        CreatedAt     = b.CreatedAt
      })
      .ToListAsync();

    return Ok(ApiResponse<List<BookingSummaryDto>>.Ok(bookings, "Lấy lịch sử thuê sân thành công."));
  }
}
