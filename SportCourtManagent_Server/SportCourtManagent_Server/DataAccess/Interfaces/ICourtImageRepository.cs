using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtImageRepository
    {
        IEnumerable<CourtImage> GetAll();
        CourtImage? GetById(int id);
        void Add(CourtImage entity);
        void Update(CourtImage entity);
        void Delete(int id);
    }
}
