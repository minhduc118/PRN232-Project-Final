using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Role>> GetAllAsync() =>
            await _context.Roles
                .Include(r => r.UserRoles)
                .OrderBy(r => r.RoleId)
                .ToListAsync();

        public Task<Role?> GetByIdAsync(int id) =>
            _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);

        public Task<Role?> GetByNameAsync(string roleName) =>
            _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);

        public async Task<int> GetAdminRoleIdAsync()
        {
            var role = await _context.Roles.FirstAsync(r => r.RoleName == "Admin");
            return role.RoleId;
        }

        public async Task<int> CountActiveAdminsAsync()
        {
            var adminRoleId = await GetAdminRoleIdAsync();
            return await _context.UserRoles
                .CountAsync(ur => ur.RoleId == adminRoleId && ur.User.IsActive);
        }
    }
}
