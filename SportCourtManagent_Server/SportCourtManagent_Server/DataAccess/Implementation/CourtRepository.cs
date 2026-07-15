using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using Microsoft.AspNetCore.Http;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtRepository : ICourtRepository
    {
        private readonly AppDbContext _context;

        public CourtRepository(AppDbContext context)
        {
            _context = context;
        }

        // --- Sync methods (legacy) ---

        public IEnumerable<Court> GetAll()
        {
            return _context.Courts.Where(c => !c.IsDeleted).ToList();
        }

        public Court? GetById(int id)
        {
            return _context.Courts.FirstOrDefault(c => c.CourtId == id && !c.IsDeleted);
        }

        public void Add(Court entity)
        {
            _context.Courts.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Court entity)
        {
            _context.Courts.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Courts.Find(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.SaveChanges();
            }
        }

        // --- Async basic methods ---

        public async Task<IEnumerable<Court>> GetAllAsync()
        {
            return await _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .Include(c => c.CourtPricings)
                .ToListAsync();
        }

        public async Task<Court?> GetByIdAsync(int id)
        {
            return await _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .Include(c => c.CourtPricings)
                .FirstOrDefaultAsync(c => c.CourtId == id);
        }

        public async Task AddAsync(Court entity)
        {
            await _context.Courts.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Court entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Courts.FindAsync(id);
            if (entity != null)
            {
                _context.Courts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // --- Advanced query methods (from main) ---

        public async Task<decimal> GetCourtPriceAsync(int courtId, int slotId, DateTime date)
        {
            var court = await GetByIdAsync(courtId);
            if (court == null)
            {
                throw new BadHttpRequestException($"Court with ID {courtId} does not exist.");
            }

            var timeSlot = await _context.TimeSlots.FindAsync(slotId);
            if (timeSlot == null)
            {
                throw new BadHttpRequestException($"Time slot with ID {slotId} does not exist.");
            }

            var courtPricing = court.CourtPricings
                .Where(cp => cp.SlotId == slotId && cp.EffectiveFrom.Date <= date.Date)
                .OrderByDescending(cp => cp.EffectiveFrom)
                .FirstOrDefault();

            if (courtPricing != null)
            {
                return courtPricing.Price;
            }

            var duration = timeSlot.EndTime - timeSlot.StartTime;
            var hours = (decimal)duration.TotalHours;
            if (hours <= 0)
            {
                hours = 1; 
            }
            return court.PricePerHour * hours;
        }

        /// <inheritdoc/>
        public IQueryable<Court> GetCourtsQueryable()
        {
            return _context.Courts
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.Complex)
                .Include(c => c.CourtType)
                .Include(c => c.CourtImages)
                .Include(c => c.CourtPricings)
                .Include(c => c.Reviews.Where(r => r.IsVisible));
        }

        /// <inheritdoc/>
        public async Task<Court?> GetCourtDetailAsync(int courtId)
        {
            return await _context.Courts
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.Complex)
                .Include(c => c.CourtType)
                .Include(c => c.CourtImages)
                .Include(c => c.CourtPricings)
                    .ThenInclude(cp => cp.TimeSlot)
                .Include(c => c.Reviews.Where(r => r.IsVisible).OrderByDescending(r => r.ReviewId).Take(5))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.CourtId == courtId);
        }

        /// <inheritdoc/>
        public async Task<Court?> GetCourtWithPricingsAsync(int courtId)
        {
            return await _context.Courts
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.CourtPricings)
                    .ThenInclude(cp => cp.TimeSlot)
                .FirstOrDefaultAsync(c => c.CourtId == courtId);
        }

        public async Task<IEnumerable<Court>> GetCourtsByComplexAsync(int complexId)
        {
            return await _context.Courts
                .Include(c => c.CourtType)
                .Where(c => c.ComplexId == complexId && !c.IsDeleted)
                .ToListAsync();
        }

        // --- Methods from feature/model-admin ---

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
