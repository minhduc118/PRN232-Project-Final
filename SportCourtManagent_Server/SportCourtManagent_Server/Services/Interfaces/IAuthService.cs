using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Auth;
using SportCourtManagent_Server.DTOs.User;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(AuthResponseDto? Data, string? Error)> LoginAsync(LoginRequest request);
        Task<string?> RegisterAsync(RegisterRequest request);
        Task<UserDto?> GetCurrentUserAsync(int userId);
        Task<(AuthResponseDto? Data, string? Error)> GoogleLoginAsync(GoogleLoginRequest request);
    }
}
