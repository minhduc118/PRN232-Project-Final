using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtPricingRepository : ICourtPricingRepository
    {
        private readonly AppDbContext _context;

        public CourtPricingRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CourtPricing> GetAll()
        {
            throw new NotImplementedException();
        }

        public CourtPricing? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(CourtPricing entity)
        {
            throw new NotImplementedException();
        }

        public void Update(CourtPricing entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
