using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class PlayerRequestRepository : IPlayerRequestRepository
    {
        private readonly AppDbContext _context;

        public PlayerRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<PlayerRequest> GetAll()
        {
            throw new NotImplementedException();
        }

        public PlayerRequest? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(PlayerRequest entity)
        {
            throw new NotImplementedException();
        }

        public void Update(PlayerRequest entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
