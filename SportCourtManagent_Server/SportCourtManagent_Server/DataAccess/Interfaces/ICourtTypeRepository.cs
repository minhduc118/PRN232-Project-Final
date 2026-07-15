using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtTypeRepository
    {
        Task<IEnumerable<CourtType>> GetAllActiveAsync();
        Task<CourtType?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
