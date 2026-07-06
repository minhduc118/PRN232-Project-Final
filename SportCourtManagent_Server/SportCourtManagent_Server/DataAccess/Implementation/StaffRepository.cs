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
            var query = _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Staff")
                    && _context.StaffComplexes.Any(sc => sc.StaffId == u.UserId && sc.ComplexId == complexId))
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u => u.FullName.Contains(term)
                    || u.Email.Contains(term)
                    || (u.Phone != null && u.Phone.Contains(term)));
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
            return await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == staffId);
        }

        public async Task<bool> IsStaffOfComplexAsync(int staffId, int complexId)
        {
            return await _context.StaffComplexes
                .AnyAsync(sc => sc.StaffId == staffId && sc.ComplexId == complexId);
        }

        public async Task AssignStaffToComplexAsync(int staffId, int complexId)
        {
            var exists = await _context.StaffComplexes
                .AnyAsync(sc => sc.StaffId == staffId && sc.ComplexId == complexId);

            if (exists)
                throw new InvalidOperationException($"Nhân viên (Id={staffId}) đã được assign vào cơ sở (Id={complexId}) trước đó.");

            _context.StaffComplexes.Add(new StaffComplex
            {
                StaffId = staffId,
                ComplexId = complexId,
                AssignedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveStaffFromComplexAsync(int staffId, int complexId)
        {
            var record = await _context.StaffComplexes
                .FirstOrDefaultAsync(sc => sc.StaffId == staffId && sc.ComplexId == complexId);

            if (record == null)
                throw new KeyNotFoundException($"Không tìm thấy assignment của nhân viên (Id={staffId}) tại cơ sở (Id={complexId}).");

            _context.StaffComplexes.Remove(record);
            await _context.SaveChangesAsync();
        }
    }
}
