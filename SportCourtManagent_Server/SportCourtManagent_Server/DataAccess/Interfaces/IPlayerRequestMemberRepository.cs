using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IPlayerRequestMemberRepository
    {
        IEnumerable<PlayerRequestMember> GetAll();
        PlayerRequestMember? GetById(int id);
        void Add(PlayerRequestMember entity);
        void Update(PlayerRequestMember entity);
        void Delete(int id);
    }
}
