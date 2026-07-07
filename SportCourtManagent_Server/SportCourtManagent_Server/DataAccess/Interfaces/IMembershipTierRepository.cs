using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface IMembershipTierRepository
    {
        Task<MembershipTier?> GetByIdAsync(int id);
        Task<MembershipTier?> GetByNameAsync(string tierName);
        Task<MembershipTier?> GetFirstAsync();
    }
}
