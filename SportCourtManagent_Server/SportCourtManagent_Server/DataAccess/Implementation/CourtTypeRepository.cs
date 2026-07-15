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

        public IEnumerable<CourtType> GetAll()
        {
            return _context.CourtTypes.Include(ct => ct.Courts).ToList();
        }

        public CourtType? GetById(int id)
        {
            return _context.CourtTypes.Include(ct => ct.Courts).FirstOrDefault(ct => ct.CourtTypeId == id);
        }

        public void Add(CourtType entity)
        {
            _context.CourtTypes.Add(entity);
            _context.SaveChanges();
        }

        public void Update(CourtType entity)
        {
            _context.CourtTypes.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                _context.CourtTypes.Remove(entity);
                _context.SaveChanges();
            }
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
