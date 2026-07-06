using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class ComplexCourtTypeServiceRepository : IComplexCourtTypeServiceRepository
    {
        private readonly AppDbContext _context;

        public ComplexCourtTypeServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ComplexCourtTypeService>> GetByComplexAndCourtTypeAsync(int complexId, int courtTypeId)
        {
            return await _context.ComplexCourtTypeServices
                .Include(o => o.Service)
                .Include(o => o.CourtType)
                .Where(o => o.ComplexId == complexId && o.CourtTypeId == courtTypeId)
                .OrderBy(o => o.ServiceMode)
                .ThenBy(o => o.Service.ServiceName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ComplexCourtTypeService>> GetByComplexAsync(int complexId)
        {
            return await _context.ComplexCourtTypeServices
                .Include(o => o.Service)
                .Include(o => o.CourtType)
                .Where(o => o.ComplexId == complexId)
                .OrderBy(o => o.CourtType.TypeName)
                .ThenBy(o => o.Service.ServiceName)
                .ToListAsync();
        }

        public Task<ComplexCourtTypeService?> GetByIdAsync(int offeringId) =>
            _context.ComplexCourtTypeServices
                .Include(o => o.Service)
                .Include(o => o.CourtType)
                .FirstOrDefaultAsync(o => o.OfferingId == offeringId);

        public async Task<bool> ExistsAsync(int complexId, int courtTypeId, int serviceId, int? excludeOfferingId = null)
        {
            var query = _context.ComplexCourtTypeServices
                .Where(o => o.ComplexId == complexId && o.CourtTypeId == courtTypeId && o.ServiceId == serviceId);
            if (excludeOfferingId.HasValue)
                query = query.Where(o => o.OfferingId != excludeOfferingId.Value);
            return await query.AnyAsync();
        }

        public async Task AddAsync(ComplexCourtTypeService entity)
        {
            await _context.ComplexCourtTypeServices.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ComplexCourtTypeService entity)
        {
            _context.ComplexCourtTypeServices.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int offeringId)
        {
            var entity = await _context.ComplexCourtTypeServices.FindAsync(offeringId);
            if (entity != null)
            {
                _context.ComplexCourtTypeServices.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
