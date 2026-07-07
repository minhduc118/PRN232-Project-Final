using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtRepository
    {
        Task<IEnumerable<Court>> GetAllWithDetailsAsync(int? complexId = null, string? status = null);
        Task<Court?> GetByIdWithDetailsAsync(int id);
        Task AddAsync(Court court);
        Task UpdateAsync(Court court);
        Task SoftDeleteAsync(int id);
        Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null);
    }
}
