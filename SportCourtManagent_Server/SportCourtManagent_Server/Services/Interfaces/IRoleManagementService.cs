using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.Authorization;
using SportCourtManagent_Server.DTOs.Role;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IRoleManagementService
    {
        Task<IReadOnlyList<RoleDto>> GetAllAsync();
        IReadOnlyList<PermissionMatrixRowDto> GetPermissionMatrix();
    }
}
