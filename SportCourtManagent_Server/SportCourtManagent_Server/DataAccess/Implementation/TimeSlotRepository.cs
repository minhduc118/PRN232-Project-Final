using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<TimeSlot> GetAll()
        {
            return _context.TimeSlots.ToList();
        }

        public TimeSlot? GetById(int id)
        {
            return _context.TimeSlots.Find(id);
        }

        public void Add(TimeSlot entity)
        {
            _context.TimeSlots.Add(entity);
            _context.SaveChanges();
        }

        public void Update(TimeSlot entity)
        {
            _context.TimeSlots.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.TimeSlots.Find(id);
            if (entity != null)
            {
                _context.TimeSlots.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
