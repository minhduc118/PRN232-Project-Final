using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<User> WithDetails() =>
            _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.MembershipTier);

        public Task<User?> GetByIdAsync(int id) =>
            _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

        public Task<User?> GetByIdWithDetailsAsync(int id) =>
            WithDetails().FirstOrDefaultAsync(u => u.UserId == id);

        public Task<User?> GetByEmailWithDetailsAsync(string email) =>
            WithDetails().FirstOrDefaultAsync(u => u.Email == email);

        public Task<bool> ExistsByEmailAsync(string email) =>
            _context.Users.AnyAsync(u => u.Email == email);

        public async Task<IReadOnlyList<User>> GetPagedWithDetailsAsync(
            string? search,
            string? role,
            bool? isActive,
            int page,
            int pageSize)
        {
            var query = ApplyFilters(WithDetails(), search, role, isActive);
            return await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> CountAsync(string? search, string? role, bool? isActive)
        {
            var query = ApplyFilters(WithDetails(), search, role, isActive);
            return query.CountAsync();
        }

        public async Task AddAsync(User entity)
        {
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User entity)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        private static IQueryable<User> ApplyFilters(
            IQueryable<User> query,
            string? search,
            string? role,
            bool? isActive)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(term) ||
                    u.Email.Contains(term) ||
                    (u.Phone != null && u.Phone.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var roleName = role.Trim();
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == roleName));
            }

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            return query;
        }
    }
}
