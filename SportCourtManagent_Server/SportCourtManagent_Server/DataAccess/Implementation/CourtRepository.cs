using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
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
                _context.Courts.Update(entity);
            await _context.SaveChangesAsync ();
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

    }
}
