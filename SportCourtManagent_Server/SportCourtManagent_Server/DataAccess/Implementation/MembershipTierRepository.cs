using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class MembershipTierRepository : IMembershipTierRepository
    {
        private readonly AppDbContext _context;

        public MembershipTierRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<MembershipTier?> GetByIdAsync(int id) =>
            _context.MembershipTiers.FirstOrDefaultAsync(t => t.TierId == id);

        public Task<MembershipTier?> GetByNameAsync(string tierName) =>
            _context.MembershipTiers.FirstOrDefaultAsync(t => t.TierName == tierName);

        public Task<MembershipTier?> GetFirstAsync() =>
            _context.MembershipTiers.OrderBy(t => t.TierId).FirstOrDefaultAsync();
    }
}
