using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtRepository : ICourtRepository
    {
        private readonly AppDbContext _context;

        public CourtRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Court>> GetAllWithDetailsAsync(int? complexId = null, string? status = null)
        {
            var query = _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .Include(c => c.CourtImages)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (complexId.HasValue)
                query = query.Where(c => c.ComplexId == complexId.Value);

            if (!string.IsNullOrWhiteSpace(status) &&
                System.Enum.TryParse<CourtStatus>(status, true, out var statusEnum))
                query = query.Where(c => c.Status == statusEnum);

            return await query.OrderBy(c => c.CourtName).ToListAsync();
        }

        public Task<Court?> GetByIdWithDetailsAsync(int id) =>
            _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .Include(c => c.CourtImages)
                .FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);

        public async Task AddAsync(Court court)
        {
            await _context.Courts.AddAsync(court);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Court court)
        {
            _context.Entry(court).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var court = await _context.Courts.FirstOrDefaultAsync(c => c.CourtId == id && !c.IsDeleted);
            if (court != null)
            {
                court.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null)
        {
            var query = _context.Courts.Where(c => c.CourtCode == courtCode && !c.IsDeleted);
            if (excludeCourtId.HasValue)
            {
                query = query.Where(c => c.CourtId != excludeCourtId.Value);
            }
            return await query.AnyAsync();
        }
    }
}

