using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Users;

namespace SportCourtManagerment.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
  private readonly ApplicationDbContext _db;

  public UsersController(ApplicationDbContext db)
  {
    _db = db;
  }

  // GET /api/users/{id}
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetById(int id)
  {
    var user = await _db.Users
      .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
      .FirstOrDefaultAsync(u => u.UserId == id);

    if (user is null)
      return NotFound(ApiResponse<object>.Fail("Không tìm thấy người dùng.", 404));

    var dto = MapToDto(user);
    return Ok(ApiResponse<UserSummaryDto>.Ok(dto, "Lấy thông tin người dùng thành công."));
  }

  // GET /api/users?role=Manager
  // role=Manager trả về Admin + Staff — những vai trò có thể quản lý tổ hợp sân
  [HttpGet]
  public async Task<IActionResult> GetList([FromQuery] string? role = null)
  {
    var query = _db.Users
      .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
      .Where(u => u.IsActive)
      .AsQueryable();

    if (!string.IsNullOrWhiteSpace(role))
    {
      var roleName = role.Trim();
      if (roleName.Equals("Manager", StringComparison.OrdinalIgnoreCase))
      {
        query = query.Where(u =>
          u.UserRoles.Any(ur =>
            ur.Role.RoleName == "Admin" || ur.Role.RoleName == "Staff"));
      }
      else
      {
        query = query.Where(u =>
          u.UserRoles.Any(ur => ur.Role.RoleName == roleName));
      }
    }

    var users = await query
      .OrderBy(u => u.FullName)
      .ToListAsync();

    var dtos = users.Select(MapToDto).ToList();
    return Ok(ApiResponse<List<UserSummaryDto>>.Ok(dtos, "Lấy danh sách người dùng thành công."));
  }

  private static UserSummaryDto MapToDto(Models.User user)
  {
    var primaryRole = user.UserRoles
      .Select(ur => ur.Role.RoleName)
      .OrderBy(r => r)
      .FirstOrDefault() ?? "Customer";

    return new UserSummaryDto
    {
      UserId    = user.UserId,
      FullName  = user.FullName,
      Email     = user.Email,
      Phone     = user.Phone,
      AvatarUrl = user.AvatarUrl,
      Role      = primaryRole,
      IsActive  = user.IsActive
    };
  }
}
