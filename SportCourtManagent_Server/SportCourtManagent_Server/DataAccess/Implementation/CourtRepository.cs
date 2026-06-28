using System;
using System.Collections.Generic;
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
            return _context.Courts;
        }

        public Court? GetById(int id)
        {
            return _context.Courts.Find(id);
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
            var court = _context.Courts.Find(id);
            if (court != null)
            {
                _context.Courts.Remove(court);
                _context.SaveChanges();
            }
        }
    }
}
