using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtTypeRepository : ICourtTypeRepository
    {
        private readonly AppDbContext _context;

        public CourtTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourtType>> GetAllActiveAsync() =>
            await _context.CourtTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.TypeName)
                .ToListAsync();

        public Task<CourtType?> GetByIdAsync(int id) =>
            _context.CourtTypes.FirstOrDefaultAsync(t => t.CourtTypeId == id);

        public Task<bool> ExistsAsync(int id) =>
            _context.CourtTypes.AnyAsync(t => t.CourtTypeId == id && t.IsActive);
    }
}

