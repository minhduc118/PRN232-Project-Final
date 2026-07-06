using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs.User;
using SportCourtManagent_Server.DTOs.Role;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Authorization;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.MembershipTier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(term) ||
                    u.Email.Contains(term) ||
                    (u.Phone != null && u.Phone.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var roleName = role.Trim();
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == roleName));
            }

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => MapSummaryDto(u))
                .ToListAsync();

            var result = new
            {
                items = users,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(ApiResults.Ok(result, "Lấy danh sách người dùng thành công."));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(ApiResults.Fail("Không tìm thấy người dùng.", 404));

            return Ok(ApiResults.Ok(MapSummaryDto(user), "Lấy thông tin người dùng thành công."));
        }

        [HttpPut("{id:int}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Role))
                return BadRequest(ApiResults.Fail("Vui lòng chọn vai trò."));

            var roleName = request.Role.Trim();
            if (!PermissionMatrix.ValidRoleNames.Contains(roleName))
                return BadRequest(ApiResults.Fail("Vai trò không hợp lệ."));

            var currentUserId = GetCurrentUserId();
            if (currentUserId == id && roleName != "Admin")
                return BadRequest(ApiResults.Fail("Bạn không thể tự hạ quyền Admin của chính mình."));

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(ApiResults.Fail("Không tìm thấy người dùng.", 404));

            var currentRole = user.UserRoles.FirstOrDefault()?.Role?.RoleName;

            if (currentRole == "Admin" && roleName != "Admin")
            {
                var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == "Admin");
                var adminCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRole.RoleId);
                if (adminCount <= 1)
                    return BadRequest(ApiResults.Fail("Không thể đổi vai trò của Admin cuối cùng trong hệ thống."));
            }

            var targetRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (targetRole == null)
                return BadRequest(ApiResults.Fail("Vai trò không tồn tại trong hệ thống."));

            var existingRoles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
            _context.UserRoles.RemoveRange(existingRoles);
            _context.UserRoles.Add(new UserRole { UserId = id, RoleId = targetRole.RoleId });
            await _context.SaveChangesAsync();

            user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.MembershipTier)
                .FirstAsync(u => u.UserId == id);

            return Ok(ApiResults.Ok(MapSummaryDto(user), "Cập nhật vai trò thành công."));
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] SetUserStatusRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == id && !request.IsActive)
                return BadRequest(ApiResults.Fail("Bạn không thể tự vô hiệu hóa tài khoản của chính mình."));

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.MembershipTier)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(ApiResults.Fail("Không tìm thấy người dùng.", 404));

            if (!request.IsActive)
            {
                var isAdmin = user.UserRoles.Any(ur => ur.Role.RoleName == "Admin");
                if (isAdmin)
                {
                    var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == "Admin");
                    var activeAdminCount = await _context.UserRoles
                        .CountAsync(ur => ur.RoleId == adminRole.RoleId && ur.User.IsActive);
                    if (activeAdminCount <= 1)
                        return BadRequest(ApiResults.Fail("Không thể vô hiệu hóa Admin cuối cùng trong hệ thống."));
                }
            }

            user.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            return Ok(ApiResults.Ok(MapSummaryDto(user),
                request.IsActive ? "Đã kích hoạt tài khoản." : "Đã vô hiệu hóa tài khoản."));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            var user = await _context.Users
                .Include(u => u.MembershipTier)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại." });
            }

            // Update allowed fields
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.AvatarUrl = request.AvatarUrl;
            user.DateOfBirth = request.DateOfBirth;

            // Map Gender enum
            if (!string.IsNullOrEmpty(request.Gender))
            {
                if (Enum.TryParse<Gender>(request.Gender, out var genderEnum))
                {
                    user.Gender = genderEnum;
                }
                else
                {
                    return BadRequest(new { message = "Giới tính không hợp lệ." });
                }
            }

            // Map SkillLevel enum if passed
            if (!string.IsNullOrEmpty(request.SkillLevel))
            {
                if (Enum.TryParse<SkillLevel>(request.SkillLevel, out var skillEnum))
                {
                    user.SkillLevel = skillEnum;
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var roleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Customer";

            var userDto = new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                LoyaltyPoints = user.LoyaltyPoints,
                MembershipTierId = user.MembershipTierId,
                MembershipTierName = user.MembershipTier?.TierName ?? "Bronze",
                Role = roleName,
                Gender = user.Gender.ToString(),
                SkillLevel = user.SkillLevel.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(new { message = "Cập nhật thông tin cá nhân thành công.", data = userDto });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại." });
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Mật khẩu hiện tại không chính xác." });
            }

            // Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thay đổi mật khẩu thành công." });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private static UserDto MapSummaryDto(User user)
        {
            var roleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Customer";
            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Role = roleName,
                MembershipTierName = user.MembershipTier?.TierName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
