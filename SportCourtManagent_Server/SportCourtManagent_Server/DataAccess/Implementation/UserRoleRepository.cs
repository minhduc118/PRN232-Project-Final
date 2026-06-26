using System;
using System.Collections.Generic;
using System.Linq;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly AppDbContext _context;

        public UserRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<UserRole> GetAll()
        {
            return _context.UserRoles.ToList();
        }

        public UserRole? GetById(int id)
        {
            return _context.UserRoles.Find(id);
        }

        public void Add(UserRole entity)
        {
            _context.UserRoles.Add(entity);
            _context.SaveChanges();
        }

        public void Update(UserRole entity)
        {
            _context.UserRoles.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.UserRoles.Find(id);
            if (entity != null)
            {
                _context.UserRoles.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
