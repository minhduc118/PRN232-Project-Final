using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IRoleRepository
    {
        Task<IReadOnlyList<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByNameAsync(string roleName);
        Task<int> GetAdminRoleIdAsync();
        Task<int> CountActiveAdminsAsync();
    }
}
