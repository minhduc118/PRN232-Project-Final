using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.CourtTypes;
using SportCourtManagerment.Enums;

namespace SportCourtManagerment.Controllers;

[ApiController]
[Route("api/court-types")]
public class CourtTypesController : ControllerBase
{
  private readonly ApplicationDbContext _db;

  public CourtTypesController(ApplicationDbContext db)
  {
    _db = db;
  }

  // ──────────────────────────────────────────────────────────────────────────
  //  GET /api/court-types
  // ──────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Returns all active court types with their respective court counts.
  /// Used in search filter sidebar and landing page category grid.
  /// </summary>
  [HttpGet]
  public async Task<IActionResult> GetCourtTypes()
  {
    var courtTypes = await _db.CourtTypes
      .AsNoTracking()
      .Where(ct => ct.IsActive)
      .Select(ct => new CourtTypeDto
      {
        CourtTypeId = ct.CourtTypeId,
        TypeName    = ct.TypeName,
        IconUrl     = ct.IconUrl,
        Description = ct.Description,
        CourtCount  = ct.Courts.Count(c => c.Status != CourtStatus.Inactive),
      })
      .OrderBy(ct => ct.TypeName)
      .ToListAsync();

    return Ok(ApiResponse<List<CourtTypeDto>>.Ok(courtTypes,
      "Lấy danh sách loại sân thành công."));
  }
}
