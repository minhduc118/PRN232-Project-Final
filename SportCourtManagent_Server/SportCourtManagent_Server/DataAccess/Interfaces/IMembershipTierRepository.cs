using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IMembershipTierRepository
    {
        IEnumerable<MembershipTier> GetAll();
        MembershipTier? GetById(int id);
        void Add(MembershipTier entity);
        void Update(MembershipTier entity);
        void Delete(int id);
    }
}
