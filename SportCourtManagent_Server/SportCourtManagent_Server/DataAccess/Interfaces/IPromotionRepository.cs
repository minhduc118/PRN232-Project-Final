using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IPromotionRepository
    {
        IEnumerable<Promotion> GetAll();
        Promotion? GetById(int id);
        void Add(Promotion entity);
        void Update(Promotion entity);
        void Delete(int id);
    }
}
