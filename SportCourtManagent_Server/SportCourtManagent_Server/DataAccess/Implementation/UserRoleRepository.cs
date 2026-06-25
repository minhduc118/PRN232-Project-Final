using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public UserRole? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(UserRole entity)
        {
            throw new NotImplementedException();
        }

        public void Update(UserRole entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
