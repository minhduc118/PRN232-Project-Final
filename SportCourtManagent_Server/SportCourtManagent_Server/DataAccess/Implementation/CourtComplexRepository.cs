using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtComplexRepository : ICourtComplexRepository
    {
        private readonly AppDbContext _context;

        public CourtComplexRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CourtComplex> GetAll()
        {
            throw new NotImplementedException();
        }

        public CourtComplex? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(CourtComplex entity)
        {
            throw new NotImplementedException();
        }

        public void Update(CourtComplex entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
