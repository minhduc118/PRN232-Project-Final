using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            return _context.StaffShifts
                .Include(s => s.Staff)
                .ToList();
        }

        public StaffShift? GetById(int id)
        {
            return _context.StaffShifts
                .Include(s => s.Staff)
                .FirstOrDefault(s => s.ShiftId == id);
        }

        public void Add(StaffShift entity)
        {
            _context.StaffShifts.Add(entity);
            _context.SaveChanges();
        }

        public void Update(StaffShift entity)
        {
            _context.StaffShifts.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.StaffShifts.Find(id);
            if (entity != null)
            {
                _context.StaffShifts.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
