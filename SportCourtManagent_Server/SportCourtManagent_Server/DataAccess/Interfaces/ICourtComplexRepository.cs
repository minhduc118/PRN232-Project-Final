using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ICourtComplexRepository
    {
        Task<IEnumerable<CourtComplex>> GetAllWithDetailsAsync();
        Task<CourtComplex?> GetByIdWithDetailsAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task AddAsync(CourtComplex entity);
        Task UpdateAsync(CourtComplex entity);
        Task SoftDeleteAsync(int id);
        Task<ComplexStatsDto> GetStatsAsync();
    }
}
