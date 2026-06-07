using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<Court> GetAll()
        {
            return _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .Include(c => c.CourtPricings)
                .ToList();
        }

        public Court? GetById(int id)
        {
            return _context.Courts
                .Include(c => c.CourtType)
                .Include(c => c.Complex)
                .Include(c => c.CourtPricings)
                .FirstOrDefault(c => c.CourtId == id);
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
                _context.Courts.Remove(entity);
                _context.SaveChanges();
            }
        }

        public decimal GetCourtPrice(int courtId, int slotId, DateTime date)
        {
            var court = GetById(courtId);
            if (court == null)
            {
                throw new BadHttpRequestException($"Court with ID {courtId} does not exist.");
            }

            var timeSlot = _context.TimeSlots.Find(slotId);
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
