using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtPricingRepository
    {
        IEnumerable<CourtPricing> GetAll();
        CourtPricing? GetById(int id);
        void Add(CourtPricing entity);
        void Update(CourtPricing entity);
        void Delete(int id);
    }
}
