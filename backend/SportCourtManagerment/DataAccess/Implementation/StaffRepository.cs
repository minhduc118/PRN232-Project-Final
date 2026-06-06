using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DataAccess.Interface;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.DataAccess.Implementation;

public class StaffRepository : IStaffRepository
{
    private readonly ApplicationDbContext _db;

    public StaffRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(List<User> Items, int TotalCount)> GetStaffByComplexAsync(
      int complexId,
      string? search = null,
      bool? isActive = null,
      int page = 1,
      int pageSize = 20)
    {
        // Lọc theo role "Staff".
        // TODO: Khi StaffShift có thêm cột ComplexId,
        //       bổ sung điều kiện: && _db.StaffShifts.Any(ss => ss.StaffId == u.UserId && ss.ComplexId == complexId)
        var query = _db.Users
          .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
          .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
          .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
              u.FullName.Contains(term) ||
              u.Email.Contains(term) ||
              (u.Phone != null && u.Phone.Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
          .OrderBy(u => u.FullName)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> IsStaffOfComplexAsync(int staffId, int complexId)
    {
        return await _db.Users
          .AnyAsync(u =>
            u.UserId == staffId &&
            u.UserRoles.Any(ur => ur.Role.RoleName == "Staff"));
    }

    public async Task<User?> GetStaffWithRolesAsync(int staffId)
    {
        return await _db.Users
          .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
          .FirstOrDefaultAsync(u => u.UserId == staffId);
    }
}
