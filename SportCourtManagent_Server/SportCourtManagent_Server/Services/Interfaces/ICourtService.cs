using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Court;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface ICourtService
    {
        Task<IEnumerable<CourtDto>> GetAllAsync(int? complexId, string? status);
        Task<CourtDto?> GetByIdAsync(int id);
        Task<CourtDto> CreateAsync(CourtDto dto);
        Task UpdateAsync(int id, CourtDto dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null);
    }
}
