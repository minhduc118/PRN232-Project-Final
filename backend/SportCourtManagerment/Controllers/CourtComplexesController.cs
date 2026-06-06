using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Courts;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.Controllers;

[ApiController]
[Route("api/complexes")]
public class CourtComplexesController : ControllerBase
{
  private readonly ApplicationDbContext _db;

  public CourtComplexesController(ApplicationDbContext db)
  {
    _db = db;
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
    // Clamp page size to reasonable bounds
    pageSize = Math.Clamp(pageSize, 1, 100);
    page     = Math.Max(1, page);

    // Base query: non-deleted complexes with their courts
    var query = _db.CourtComplexes
      .Include(cx => cx.Courts)
      .Where(cx => !cx.IsDeleted)
      .AsQueryable();

    // 1️⃣ Full-text search across name / address / phone / manager
    if (!string.IsNullOrWhiteSpace(search))
    {
      var term = search.Trim();
      query = query.Where(cx =>
        cx.ComplexName.Contains(term) ||
        cx.Address.Contains(term) ||
        (cx.Phone != null && cx.Phone.Contains(term)) ||
        (cx.ManagerName != null && cx.ManagerName.Contains(term)));
    }

    // 2️⃣ Filter by court type: keep only complexes that have at least one court of that type
    if (courtTypeId.HasValue)
    {
      query = query.Where(cx =>
        cx.Courts.Any(c => !c.IsDeleted && c.CourtTypeId == courtTypeId.Value));
    }

    // 3️⃣ Total count for pagination meta (before paging)
    var totalCount = await query.CountAsync();

    // 4️⃣ Order + page
    var items = await query
      .OrderByDescending(cx => cx.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Select(cx => new CourtComplexDto
      {
        ComplexId         = cx.ComplexId,
        ComplexName       = cx.ComplexName,
        Address           = cx.Address,
        Phone             = cx.Phone,
        ManagerName       = cx.ManagerName,
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

    var totalComplexes = await _db.CourtComplexes.CountAsync(cx => !cx.IsDeleted);
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
  // Aggregate statistics across the entire system
  // ─────────────────────────────────────────────
  [HttpGet("stats")]
  public async Task<IActionResult> GetStats()
  {
    var totalComplexes = await _db.CourtComplexes.CountAsync(cx => !cx.IsDeleted);

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
      .FirstOrDefaultAsync(c => c.ComplexId == id && !c.IsDeleted);

    if (cx is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy tổ hợp sân hoặc đã bị xóa.", 404));

    var dto = new CourtComplexDto
    {
      ComplexId         = cx.ComplexId,
      ComplexName       = cx.ComplexName,
      Address           = cx.Address,
      Phone             = cx.Phone,
      ManagerName       = cx.ManagerName,
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

    var cx = new CourtComplex
    {
      ComplexName = dto.ComplexName,
      Address     = dto.Address,
      Phone       = dto.Phone,
      ManagerName = dto.ManagerName,
      ManagerId   = dto.ManagerId,
      Description = dto.Description,
      ImageUrl    = dto.ImageUrl,
      CreatedAt   = DateTime.UtcNow
    };

    if (dto.ManagerId.HasValue)
    {
      var manager = await _db.Users.FindAsync(dto.ManagerId.Value);
      if (manager is not null)
        cx.ManagerName = manager.FullName;
    }

    _db.CourtComplexes.Add(cx);
    await _db.SaveChangesAsync();

    var result = new CourtComplexDto
    {
      ComplexId         = cx.ComplexId,
      ComplexName       = cx.ComplexName,
      Address           = cx.Address,
      Phone             = cx.Phone,
      ManagerName       = cx.ManagerName,
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

    var cx = await _db.CourtComplexes.FirstOrDefaultAsync(c => c.ComplexId == id && !c.IsDeleted);
    if (cx is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy tổ hợp sân cần cập nhật.", 404));

    cx.ComplexName = dto.ComplexName;
    cx.Address     = dto.Address;
    cx.Phone       = dto.Phone;
    cx.ManagerName = dto.ManagerName;
    cx.ManagerId   = dto.ManagerId;
    cx.Description = dto.Description;
    cx.ImageUrl    = dto.ImageUrl;
    cx.UpdatedAt   = DateTime.UtcNow;

    if (dto.ManagerId.HasValue)
    {
      var manager = await _db.Users.FindAsync(dto.ManagerId.Value);
      if (manager is not null)
        cx.ManagerName = manager.FullName;
    }
    else
    {
      cx.ManagerName = null;
    }

    await _db.SaveChangesAsync();

    var updatedDto = new CourtComplexDto
    {
      ComplexId         = cx.ComplexId,
      ComplexName       = cx.ComplexName,
      Address           = cx.Address,
      Phone             = cx.Phone,
      ManagerName       = cx.ManagerName,
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

    // Kiểm tra xem tổ hợp này có sân con nào chưa bị xóa mềm không
    var hasActiveCourts = cx.Courts.Any(c => !c.IsDeleted);
    if (hasActiveCourts)
    {
      return BadRequest(ApiResponse<object>.Fail(
        "Không thể xóa tổ hợp sân khi đang có các sân hoạt động bên trong. Vui lòng di chuyển hoặc xóa hết các sân con trước.", 400));
    }

    // Xóa mềm tổ hợp
    cx.IsDeleted = true;
    cx.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    return Ok(ApiResponse<object>.Ok(null, "Xóa tổ hợp sân thành công."));
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
