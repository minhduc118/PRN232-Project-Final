using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class StaffShiftRepository : IStaffShiftRepository
    {
        private readonly AppDbContext _context;

        public StaffShiftRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<StaffShift> GetAll()
        {
            throw new NotImplementedException();
        }

        public StaffShift? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(StaffShift entity)
        {
            throw new NotImplementedException();
        }

        public void Update(StaffShift entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
