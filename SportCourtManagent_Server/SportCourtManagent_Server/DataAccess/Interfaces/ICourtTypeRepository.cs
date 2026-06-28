using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtTypeRepository
    {
        IEnumerable<CourtType> GetAll();
        CourtType? GetById(int id);
        void Add(CourtType entity);
        void Update(CourtType entity);
        void Delete(int id);
    }
}
