using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class StaffRepository : IStaffRepository
    {
        private readonly AppDbContext _context;

        public StaffRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<User> Items, int TotalCount)> GetStaffByComplexAsync(int complexId, string? search = null, bool? isActive = null, int page = 1, int pageSize = 10)
        {
            var query = _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Staff")
                && _context.StaffShifts.Any(ss => ss.ComplexId == complexId && ss.StaffId == u.UserId))
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u => u.FullName.Contains(term) || u.Email.Contains(term) || (u.Phone != null && u.Phone.Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<User?> GetStaffWithRolesAsync(int staffId)
        {
            return await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == staffId);
        }

        public async Task<bool> IsStaffOfComplexAsync(int staffId, int complexId)
        {
            return await _context.StaffShifts.AnyAsync(ss => ss.StaffId == staffId && ss.ComplexId == complexId);
        }


    }
}
