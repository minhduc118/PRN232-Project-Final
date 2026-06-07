using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;
using SportCourtManagerment.DataAccess.Interface;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.DataAccess.Implementation;

public class MaintenanceRepository : IMaintenanceRepository
{
    private readonly ApplicationDbContext _db;

    public MaintenanceRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<MaintenanceSchedule?> GetByIdAsync(int id)
    {
        return await _db.MaintenanceSchedules
            .Include(ms => ms.Court)
                .ThenInclude(c => c.Complex)
            .Include(ms => ms.AssignedStaff)
            .FirstOrDefaultAsync(ms => ms.MaintenanceId == id);
    }

    public async Task<(List<MaintenanceSchedule> Items, int TotalCount)> GetByComplexAsync(
        int complexId,
        MaintenanceStatus? status = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _db.MaintenanceSchedules
            .Include(ms => ms.Court)
                .ThenInclude(c => c.Complex)
            .Include(ms => ms.AssignedStaff)
            .Where(ms => ms.Court.ComplexId == complexId)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(ms => ms.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(ms => ms.StartDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<MaintenanceSchedule> CreateAsync(MaintenanceSchedule schedule)
    {
        _db.MaintenanceSchedules.Add(schedule);
        await _db.SaveChangesAsync();
        return schedule;
    }

    public async Task<bool> UpdateAsync(MaintenanceSchedule schedule)
    {
        _db.MaintenanceSchedules.Update(schedule);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(MaintenanceSchedule schedule)
    {
        _db.MaintenanceSchedules.Remove(schedule);
        return await _db.SaveChangesAsync() > 0;
    }
}
