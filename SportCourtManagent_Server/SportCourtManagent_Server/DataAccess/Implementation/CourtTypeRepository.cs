using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class CourtTypeRepository : ICourtTypeRepository
    {
        private readonly AppDbContext _context;

        public CourtTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CourtType> GetAll()
        {
            throw new NotImplementedException();
        }

        public CourtType? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(CourtType entity)
        {
            throw new NotImplementedException();
        }

        public void Update(CourtType entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
