using System;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.User;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<object> GetPagedAsync(string? search, string? role, bool? isActive, int page, int pageSize);
        Task<List<UserDto>> GetManagersAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<(UserDto? Data, string? Error)> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<string?> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<(UserDto? Data, string? Error)> CreateAsync(CreateUserRequest request);
        Task<(UserDto? Data, string? Error)> UpdateUserByAdminAsync(int userId, UpdateUserByAdminRequest request);
        Task<string?> DeleteAsync(int userId);
    }
}
