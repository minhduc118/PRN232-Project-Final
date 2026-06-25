using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IPlayerRequestRepository
    {
        IEnumerable<PlayerRequest> GetAll();
        PlayerRequest? GetById(int id);
        void Add(PlayerRequest entity);
        void Update(PlayerRequest entity);
        void Delete(int id);
    }
}
