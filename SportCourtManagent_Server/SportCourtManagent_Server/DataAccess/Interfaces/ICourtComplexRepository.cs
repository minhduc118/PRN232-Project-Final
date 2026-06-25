using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtComplexRepository
    {
        IEnumerable<CourtComplex> GetAll();
        CourtComplex? GetById(int id);
        void Add(CourtComplex entity);
        void Update(CourtComplex entity);
        void Delete(int id);
    }
}
