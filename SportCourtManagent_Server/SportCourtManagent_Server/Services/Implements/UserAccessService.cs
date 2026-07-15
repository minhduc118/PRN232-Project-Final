using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.Authorization;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Role;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class UserAccessService : IUserAccessService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;

        public UserAccessService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
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

            var user = await _userRepo.GetByIdWithDetailsAsync(targetUserId);
            if (user == null)
                return (null, "Không tìm thấy người dùng.");

            var currentRole = user.UserRoles.FirstOrDefault()?.Role?.RoleName;
            var isCurrentlyAdmin = currentRole == "Admin";
            var willRemainActiveAdmin = roleName == "Admin" && request.IsActive;

            if (isCurrentlyAdmin && !willRemainActiveAdmin)
            {
                var activeAdminCount = await _roleRepo.CountActiveAdminsAsync();
                if (activeAdminCount <= 1)
                    return (null, "Không thể thay đổi — hệ thống cần ít nhất một Admin đang hoạt động.");
            }

            var targetRole = await _roleRepo.GetByNameAsync(roleName);
            if (targetRole == null)
                return (null, "Vai trò không tồn tại trong hệ thống.");

            await _userRoleRepo.ReplaceUserRoleAsync(targetUserId, targetRole.RoleId);

            user.IsActive = request.IsActive;
            await _userRepo.UpdateAsync(user);

            user = await _userRepo.GetByIdWithDetailsAsync(targetUserId);
            return (user, null);
        }

        public async Task<(User? User, string? Error)> SetStatusAsync(int targetUserId, int? actorUserId, bool isActive)
        {
            if (actorUserId == targetUserId && !isActive)
                return (null, "Bạn không thể tự vô hiệu hóa tài khoản của chính mình.");

            var user = await _userRepo.GetByIdWithDetailsAsync(targetUserId);
            if (user == null)
                return (null, "Không tìm thấy người dùng.");

            if (!isActive && user.UserRoles.Any(ur => ur.Role.RoleName == "Admin"))
            {
                var activeAdminCount = await _roleRepo.CountActiveAdminsAsync();
                if (activeAdminCount <= 1)
                    return (null, "Không thể vô hiệu hóa Admin cuối cùng đang hoạt động.");
            }

            user.IsActive = isActive;
            await _userRepo.UpdateAsync(user);
            return (user, null);
        }

        public async Task<(User? User, string? Error)> AssignRoleAsync(int targetUserId, int? actorUserId, string role)
        {
            var user = await _userRepo.GetByIdWithDetailsAsync(targetUserId);
            if (user == null)
                return (null, "Không tìm thấy người dùng.");

            return await UpdateAccessAsync(targetUserId, actorUserId, new UpdateUserAccessRequest
            {
                Role = role,
                IsActive = user.IsActive
            });
        }
    }
}
