using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtImageRepository : ICourtImageRepository
    {
        private readonly AppDbContext _context;

        public CourtImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CourtImage> GetAll()
        {
            throw new NotImplementedException();
        }

        public CourtImage? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(CourtImage entity)
        {
            throw new NotImplementedException();
        }

        public void Update(CourtImage entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
