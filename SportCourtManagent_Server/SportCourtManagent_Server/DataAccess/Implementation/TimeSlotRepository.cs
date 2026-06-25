using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public TimeSlot? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(TimeSlot entity)
        {
            throw new NotImplementedException();
        }

        public void Update(TimeSlot entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
