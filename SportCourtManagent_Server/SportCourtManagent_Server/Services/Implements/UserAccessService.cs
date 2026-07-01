using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Authorization;
using SportCourtManagent_Server.DTOs.Role;
using SportCourtManagent_Server.DTOs.User;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Services.Implements
{
    public class UserAccessService
    {
        private readonly AppDbContext _context;

        public UserAccessService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(User? User, string? Error)> UpdateAccessAsync(
            int targetUserId,
            int? actorUserId,
            UpdateUserAccessRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Role))
                return (null, "Vui lòng chọn vai trò.");

            var roleName = request.Role.Trim();
            if (!PermissionMatrix.ValidRoleNames.Contains(roleName))
                return (null, "Vai trò không hợp lệ.");

            if (actorUserId == targetUserId)
            {
                if (roleName != "Admin")
                    return (null, "Bạn không thể tự hạ quyền Admin của chính mình.");
                if (!request.IsActive)
                    return (null, "Bạn không thể tự vô hiệu hóa tài khoản của chính mình.");
            }

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.MembershipTier)
                .FirstOrDefaultAsync(u => u.UserId == targetUserId);

            if (user == null)
                return (null, "Không tìm thấy người dùng.");

            var currentRole = user.UserRoles.FirstOrDefault()?.Role?.RoleName;
            var isCurrentlyAdmin = currentRole == "Admin";
            var willRemainActiveAdmin = roleName == "Admin" && request.IsActive;

            if (isCurrentlyAdmin && !willRemainActiveAdmin)
            {
                var activeAdminCount = await CountActiveAdminsAsync();
                if (activeAdminCount <= 1)
                    return (null, "Không thể thay đổi — hệ thống cần ít nhất một Admin đang hoạt động.");
            }

            var targetRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (targetRole == null)
                return (null, "Vai trò không tồn tại trong hệ thống.");

            var existingRoles = await _context.UserRoles.Where(ur => ur.UserId == targetUserId).ToListAsync();
            _context.UserRoles.RemoveRange(existingRoles);
            _context.UserRoles.Add(new UserRole { UserId = targetUserId, RoleId = targetRole.RoleId });

            user.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.MembershipTier)
                .FirstAsync(u => u.UserId == targetUserId);

            return (user, null);
        }

        public async Task<(User? User, string? Error)> SetStatusAsync(int targetUserId, int? actorUserId, bool isActive)
        {
            if (actorUserId == targetUserId && !isActive)
                return (null, "Bạn không thể tự vô hiệu hóa tài khoản của chính mình.");

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.MembershipTier)
                .FirstOrDefaultAsync(u => u.UserId == targetUserId);

            if (user == null)
                return (null, "Không tìm thấy người dùng.");

            if (!isActive && user.UserRoles.Any(ur => ur.Role.RoleName == "Admin"))
            {
                var activeAdminCount = await CountActiveAdminsAsync();
                if (activeAdminCount <= 1)
                    return (null, "Không thể vô hiệu hóa Admin cuối cùng đang hoạt động.");
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();
            return (user, null);
        }

        public async Task<(User? User, string? Error)> AssignRoleAsync(int targetUserId, int? actorUserId, string role)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == targetUserId);

            if (user == null)
                return (null, "Không tìm thấy người dùng.");

            return await UpdateAccessAsync(targetUserId, actorUserId, new UpdateUserAccessRequest
            {
                Role = role,
                IsActive = user.IsActive
            });
        }

        private async Task<int> CountActiveAdminsAsync()
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.RoleName == "Admin")
                .Select(r => r.RoleId)
                .FirstAsync();

            return await _context.UserRoles
                .CountAsync(ur => ur.RoleId == adminRoleId && ur.User.IsActive);
        }

        public static UserDto MapSummaryDto(User user)
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
