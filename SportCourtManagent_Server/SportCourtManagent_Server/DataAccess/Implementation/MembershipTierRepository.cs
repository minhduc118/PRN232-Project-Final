using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class MembershipTierRepository : IMembershipTierRepository
    {
        private readonly AppDbContext _context;

        public MembershipTierRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<MembershipTier> GetAll()
        {
            throw new NotImplementedException();
        }

        public MembershipTier? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(MembershipTier entity)
        {
            throw new NotImplementedException();
        }

        public void Update(MembershipTier entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
