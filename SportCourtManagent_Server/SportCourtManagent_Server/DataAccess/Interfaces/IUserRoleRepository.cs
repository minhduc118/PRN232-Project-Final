using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<IReadOnlyList<UserRole>> GetByUserIdAsync(int userId);
        Task ReplaceUserRoleAsync(int userId, int roleId);
        Task AddAsync(UserRole entity);
    }
}
