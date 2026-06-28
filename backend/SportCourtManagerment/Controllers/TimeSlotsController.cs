using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;

namespace SportCourtManagerment.Controllers;

[ApiController]
[Route("api/timeslots")]
public class TimeSlotsController : ControllerBase
{
  private readonly ApplicationDbContext _db;

  public TimeSlotsController(ApplicationDbContext db)
  {
    _db = db;
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var slots = await _db.TimeSlots
      .Where(t => t.IsActive)
      .OrderBy(t => t.StartTime)
      .ToListAsync();

    return Ok(ApiResponse<object>.Ok(slots, "Lấy danh sách khung giờ thành công."));
  }
}
