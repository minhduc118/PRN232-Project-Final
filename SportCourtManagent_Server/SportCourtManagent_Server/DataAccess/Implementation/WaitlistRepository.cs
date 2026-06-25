using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class WaitlistRepository : IWaitlistRepository
    {
        private readonly AppDbContext _context;

        public WaitlistRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Waitlist> GetAll()
        {
            throw new NotImplementedException();
        }

        public Waitlist? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(Waitlist entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Waitlist entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
