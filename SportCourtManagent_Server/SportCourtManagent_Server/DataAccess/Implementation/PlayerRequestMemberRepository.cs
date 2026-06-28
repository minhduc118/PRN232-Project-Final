using System;
using System.Collections.Generic;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class PlayerRequestMemberRepository : IPlayerRequestMemberRepository
    {
        private readonly AppDbContext _context;

        public PlayerRequestMemberRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<PlayerRequestMember> GetAll()
        {
            throw new NotImplementedException();
        }

        public PlayerRequestMember? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(PlayerRequestMember entity)
        {
            throw new NotImplementedException();
        }

        public void Update(PlayerRequestMember entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
