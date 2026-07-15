using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Role;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IUserAccessService
    {
        Task<(User? User, string? Error)> UpdateAccessAsync(
            int targetUserId,
            int? actorUserId,
            UpdateUserAccessRequest request);

        Task<(User? User, string? Error)> SetStatusAsync(int targetUserId, int? actorUserId, bool isActive);

        Task<(User? User, string? Error)> AssignRoleAsync(int targetUserId, int? actorUserId, string role);
    }
}
