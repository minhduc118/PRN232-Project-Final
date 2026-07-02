using System;
using System.Collections.Generic;
using System.Linq;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Role> GetAll()
        {
            return _context.Roles.ToList();
        }

        public Role? GetById(int id)
        {
            return _context.Roles.Find(id);
        }

        public void Add(Role entity)
        {
            _context.Roles.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Role entity)
        {
            _context.Roles.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Roles.Find(id);
            if (entity != null)
            {
                _context.Roles.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
