using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.Authorization;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Role;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleRepository _roleRepo;

        public RoleManagementService(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<IReadOnlyList<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepo.GetAllAsync();
            return roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                UserCount = r.UserRoles.Count
            }).ToList();
        }

        public IReadOnlyList<PermissionMatrixRowDto> GetPermissionMatrix() =>
            PermissionMatrix.GetRows();
    }
}
