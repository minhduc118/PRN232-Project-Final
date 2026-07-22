using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Role;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IPermissionMatrixRepository _permissionRepo;

        public RoleManagementService(IRoleRepository roleRepo, IPermissionMatrixRepository permissionRepo)
        {
            _roleRepo = roleRepo;
            _permissionRepo = permissionRepo;
        }

        public async Task<IReadOnlyList<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepo.GetAllAsync();
            return roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = !string.IsNullOrWhiteSpace(r.Description) ? r.Description : GetDefaultRoleDescription(r.RoleName),
                UserCount = r.UserRoles.Count
            }).ToList();
        }

        private static string GetDefaultRoleDescription(string roleName) => roleName switch
        {
            "Admin"    => "Quản trị viên toàn hệ thống, toàn quyền quản lý.",
            "Manager"  => "Quản lý phụ trách tổ hợp sân thể thao.",
            "Staff"    => "Nhân viên vận hành, hỗ trợ đặt sân và khách hàng.",
            "Customer" => "Khách hàng sử dụng dịch vụ và đặt sân.",
            _          => "Vai trò người dùng trong hệ thống."
        };

        public async Task<IReadOnlyList<PermissionMatrixRowDto>> GetPermissionMatrixAsync()
        {
            var entries = await _permissionRepo.GetAllAsync();
            return entries.Select(e => new PermissionMatrixRowDto
            {
                Feature  = e.Feature,
                Admin    = e.Admin,
                Manager  = e.Manager,
                Staff    = e.Staff,
                Customer = e.Customer
            }).ToList();
        }

        public async Task UpdatePermissionMatrixAsync(List<PermissionMatrixRowDto> rows)
        {
            if (rows == null || rows.Count == 0) return;

            var entries = rows.Select(r => new PermissionMatrixEntry
            {
                Feature  = r.Feature,
                Admin    = r.Admin,
                Manager  = r.Manager,
                Staff    = r.Staff,
                Customer = r.Customer
            }).ToList();

            await _permissionRepo.UpsertAllAsync(entries);
        }
    }
}
