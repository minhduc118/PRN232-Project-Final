using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllAsync(bool activeOnly = false)
        {
            var query = _context.Services.AsQueryable();
            if (activeOnly)
                query = query.Where(s => s.IsActive);
            return await query.OrderBy(s => s.ServiceName).ToListAsync();
        }

        public Task<Service?> GetByIdAsync(int id) =>
            _context.Services.FirstOrDefaultAsync(s => s.ServiceId == id);

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            var query = _context.Services.Where(s => s.ServiceName.ToLower() == normalized);
            if (excludeId.HasValue)
                query = query.Where(s => s.ServiceId != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task AddAsync(Service entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _context.Services.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service entity)
        {
            _context.Services.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Services.FindAsync(id);
            if (entity != null)
            {
                entity.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
