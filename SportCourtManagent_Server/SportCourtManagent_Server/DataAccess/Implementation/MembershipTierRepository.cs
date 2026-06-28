using System;
using System.Collections.Generic;
using System.Linq;
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
            return _context.MembershipTiers.ToList();
        }

        public MembershipTier? GetById(int id)
        {
            return _context.MembershipTiers.Find(id);
        }

        public void Add(MembershipTier entity)
        {
            _context.MembershipTiers.Add(entity);
            _context.SaveChanges();
        }

        public void Update(MembershipTier entity)
        {
            _context.MembershipTiers.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.MembershipTiers.Find(id);
            if (entity != null)
            {
                _context.MembershipTiers.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
