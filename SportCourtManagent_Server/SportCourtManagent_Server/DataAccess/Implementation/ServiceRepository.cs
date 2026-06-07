using System;
using System.Collections.Generic;
using System.Linq;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Service> GetAll()
        {
            return _context.Services.ToList();
        }

        public Service? GetById(int id)
        {
            return _context.Services.Find(id);
        }

        public void Add(Service entity)
        {
            _context.Services.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Service entity)
        {
            _context.Services.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Services.Find(id);
            if (entity != null)
            {
                _context.Services.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
