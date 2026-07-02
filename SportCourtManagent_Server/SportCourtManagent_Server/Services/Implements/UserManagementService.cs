using System;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.User;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepo;

        public UserManagementService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
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
    }
}
