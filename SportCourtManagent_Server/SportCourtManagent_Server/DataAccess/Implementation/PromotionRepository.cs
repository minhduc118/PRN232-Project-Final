using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly AppDbContext _context;

        public PromotionRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Promotion> GetAll()
        {
            throw new NotImplementedException();
        }

        public Promotion? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(Promotion entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Promotion entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
