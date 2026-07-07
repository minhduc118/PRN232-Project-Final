using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class TimeSlotRepository : ITimeSlotRepository
    {
        private readonly AppDbContext _context;

        public TimeSlotRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TimeSlot>> GetAllAsync()
        {
            return await _context.TimeSlots.ToListAsync();
        }

        public async Task<TimeSlot?> GetByIdAsync(int id)
        {
            return await _context.TimeSlots.FindAsync(id);
        }

        public async Task AddAsync(TimeSlot entity)
        {
            _context.TimeSlots.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TimeSlot entity)
        {
            _context.TimeSlots.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.TimeSlots.FindAsync(id);
            if (entity != null)
            {
                _context.TimeSlots.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
