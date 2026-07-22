using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.User;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IMembershipTierRepository _tierRepo;

        public UserManagementService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IUserRoleRepository userRoleRepo,
            IMembershipTierRepository tierRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _tierRepo = tierRepo;
        }

        public async Task<object> GetPagedAsync(string? search, string? role, bool? isActive, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var totalCount = await _userRepo.CountAsync(search, role, isActive);
            var users = await _userRepo.GetPagedWithDetailsAsync(search, role, isActive, page, pageSize);

            return new
            {
                items = users.Select(UserMapper.ToSummaryDto).ToList(),
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<List<UserDto>> GetManagersAsync()
        {
            // Lấy tất cả user có vai trò Staff hoặc Manager đang active, không phân trang
            var staffUsers = await _userRepo.GetPagedWithDetailsAsync(null, "Staff", true, 1, 200);
            var managerUsers = await _userRepo.GetPagedWithDetailsAsync(null, "Manager", true, 1, 200);

            var all = staffUsers.Concat(managerUsers)
                .GroupBy(u => u.UserId)
                .Select(g => g.First())
                .ToList();

            return all.Select(UserMapper.ToSummaryDto).ToList();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _userRepo.GetByIdWithDetailsAsync(id);
            return user == null ? null : UserMapper.ToSummaryDto(user);
        }

        public async Task<(UserDto? Data, string? Error)> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _userRepo.GetByIdWithDetailsAsync(userId);
            if (user == null)
                return (null, "Người dùng không tồn tại.");

            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.AvatarUrl = request.AvatarUrl;
            user.DateOfBirth = request.DateOfBirth;

            if (!string.IsNullOrEmpty(request.Gender))
            {
                if (!Enum.TryParse<Gender>(request.Gender, out var genderEnum))
                    return (null, "Giới tính không hợp lệ.");
                user.Gender = genderEnum;
            }

            if (!string.IsNullOrEmpty(request.SkillLevel) &&
                Enum.TryParse<SkillLevel>(request.SkillLevel, out var skillEnum))
            {
                user.SkillLevel = skillEnum;
            }

            await _userRepo.UpdateAsync(user);
            user = await _userRepo.GetByIdWithDetailsAsync(userId);
            return (UserMapper.ToFullDto(user!), null);
        }

        public async Task<string?> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return "Người dùng không tồn tại.";

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                return "Mật khẩu hiện tại không chính xác.";

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepo.UpdateAsync(user);
            return null;
        }

        public async Task<(UserDto? Data, string? Error)> CreateAsync(CreateUserRequest request)
        {
            if (await _userRepo.ExistsByEmailAsync(request.Email))
                return (null, "Email này đã được đăng ký.");

            if (!string.IsNullOrWhiteSpace(request.Phone) && await _userRepo.ExistsByPhoneAsync(request.Phone))
                return (null, "Số điện thoại này đã được đăng ký.");

            var defaultTier = await _tierRepo.GetByNameAsync("Bronze")
                ?? await _tierRepo.GetFirstAsync();

            if (!Enum.TryParse<Gender>(request.Gender, out var genderEnum))
                genderEnum = Gender.Other;

            if (!Enum.TryParse<SkillLevel>(request.SkillLevel, out var skillEnum))
                skillEnum = SkillLevel.Beginner;

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLower(),
                Phone = request.Phone.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                MembershipTierId = defaultTier?.TierId,
                Gender = genderEnum,
                SkillLevel = skillEnum,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);

            var roleName = request.Role.Trim();
            var role = await _roleRepo.GetByNameAsync(roleName);
            if (role != null)
            {
                await _userRoleRepo.AddAsync(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = role.RoleId
                });
            }

            var createdUser = await _userRepo.GetByIdWithDetailsAsync(user.UserId);
            return (createdUser == null ? null : UserMapper.ToFullDto(createdUser), null);
        }

        public async Task<(UserDto? Data, string? Error)> UpdateUserByAdminAsync(int userId, UpdateUserByAdminRequest request)
        {
            var user = await _userRepo.GetByIdWithDetailsAsync(userId);
            if (user == null)
                return (null, "Người dùng không tồn tại.");

            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepo.ExistsByEmailAsync(request.Email))
                    return (null, "Email này đã được đăng ký bởi người dùng khác.");
            }

            if (!string.Equals(user.Phone, request.Phone, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(request.Phone) && await _userRepo.ExistsByPhoneAsync(request.Phone))
                    return (null, "Số điện thoại này đã được đăng ký bởi người dùng khác.");
            }

            var currentRole = user.UserRoles.FirstOrDefault()?.Role?.RoleName;
            var isCurrentlyAdmin = currentRole == "Admin";
            var willRemainActiveAdmin = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase) && request.IsActive;

            if (isCurrentlyAdmin && !willRemainActiveAdmin)
            {
                var activeAdminCount = await _roleRepo.CountActiveAdminsAsync();
                if (activeAdminCount <= 1)
                    return (null, "Không thể thay đổi — hệ thống cần ít nhất một Admin đang hoạt động.");
            }

            user.FullName = request.FullName.Trim();
            user.Email = request.Email.Trim().ToLower();
            user.Phone = request.Phone.Trim();
            user.IsActive = request.IsActive;

            if (Enum.TryParse<Gender>(request.Gender, out var genderEnum))
                user.Gender = genderEnum;

            if (Enum.TryParse<SkillLevel>(request.SkillLevel, out var skillEnum))
                user.SkillLevel = skillEnum;

            await _userRepo.UpdateAsync(user);

            var role = await _roleRepo.GetByNameAsync(request.Role.Trim());
            if (role != null)
            {
                await _userRoleRepo.ReplaceUserRoleAsync(userId, role.RoleId);
            }

            var updatedUser = await _userRepo.GetByIdWithDetailsAsync(userId);
            return (updatedUser == null ? null : UserMapper.ToFullDto(updatedUser), null);
        }

        public async Task<string?> DeleteAsync(int userId)
        {
            var user = await _userRepo.GetByIdWithDetailsAsync(userId);
            if (user == null)
                return "Người dùng không tồn tại.";

            var currentRole = user.UserRoles.FirstOrDefault()?.Role?.RoleName;
            if (currentRole == "Admin")
            {
                var activeAdminCount = await _roleRepo.CountActiveAdminsAsync();
                if (activeAdminCount <= 1)
                    return "Không thể xóa Admin cuối cùng của hệ thống.";
            }

            try
            {
                await _userRepo.DeleteAsync(user);
                return null;
            }
            catch (Exception)
            {
                return "Không thể xóa người dùng này vì họ có lịch sử hoạt động (đặt sân, thanh toán, v.v.). Vui lòng chọn Khóa/Vô hiệu hóa tài khoản thay thế.";
            }
        }
    }
}
