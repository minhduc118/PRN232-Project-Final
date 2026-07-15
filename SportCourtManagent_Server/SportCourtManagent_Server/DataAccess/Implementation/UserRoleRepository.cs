using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly AppDbContext _context;

        public UserRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<UserRole>> GetByUserIdAsync(int userId) =>
            await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();

        public async Task ReplaceUserRoleAsync(int userId, int roleId)
        {
            var existing = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _context.UserRoles.RemoveRange(existing);
            await _context.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = roleId });
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(UserRole entity)
        {
            await _context.UserRoles.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
    }
}
