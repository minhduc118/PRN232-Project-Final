using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtComplexRepository : ICourtComplexRepository
    {
        private readonly AppDbContext _context;

        public CourtComplexRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourtComplex>> GetAllWithDetailsAsync()
        {
            return await _context.CourtComplexes
                .Include(cx => cx.Courts)
                .Include(cx => cx.Manager)
                .Where(cx => !cx.IsDeleted)
                .OrderByDescending(cx => cx.CreatedAt)
                .ToListAsync();
        }

        public async Task<CourtComplex?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.CourtComplexes
                .Include(cx => cx.Courts)
                .Include(cx => cx.Manager)
                .FirstOrDefaultAsync(cx => cx.ComplexId == id && !cx.IsDeleted);
        }

        public Task<bool> ExistsAsync(int id) =>
            _context.CourtComplexes.AnyAsync(cx => cx.ComplexId == id && !cx.IsDeleted);

        public async Task AddAsync(CourtComplex entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _context.CourtComplexes.AddAsync(entity);
            await _context.SaveChangesAsync();
            // Reload navigation properties
            await _context.Entry(entity).Reference(c => c.Manager).LoadAsync();
            await _context.Entry(entity).Collection(c => c.Courts).LoadAsync();
        }

        public async Task UpdateAsync(CourtComplex entity)
        {
            _context.CourtComplexes.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await _context.CourtComplexes.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ComplexStatsDto> GetStatsAsync()
        {
            return new ComplexStatsDto
            {
                TotalComplexes = await _context.CourtComplexes.CountAsync(cx => !cx.IsDeleted),
                TotalCourts = await _context.Courts.CountAsync(c => !c.IsDeleted),
                ActiveCourts = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Available),
                MaintenanceCourts = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Maintenance),
                InactiveCourts = await _context.Courts.CountAsync(c => !c.IsDeleted && c.Status == CourtStatus.Inactive)
            };
        }
    }
}

