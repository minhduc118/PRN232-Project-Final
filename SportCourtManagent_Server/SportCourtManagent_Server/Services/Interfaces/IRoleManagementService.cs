using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Role;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IRoleManagementService
    {
        Task<IReadOnlyList<RoleDto>> GetAllAsync();
        Task<IReadOnlyList<PermissionMatrixRowDto>> GetPermissionMatrixAsync();
        Task UpdatePermissionMatrixAsync(List<PermissionMatrixRowDto> rows);
    }
}
