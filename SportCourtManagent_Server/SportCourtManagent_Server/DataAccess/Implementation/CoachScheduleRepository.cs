using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CoachScheduleRepository : ICoachScheduleRepository
    {
        private readonly AppDbContext _context;

        public CoachScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CoachSchedule> GetAll()
        {
            throw new NotImplementedException();
        }

        public CoachSchedule? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(CoachSchedule entity)
        {
            throw new NotImplementedException();
        }

        public void Update(CoachSchedule entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
