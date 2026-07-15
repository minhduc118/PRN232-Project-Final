using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, string roleName);
    }
}
