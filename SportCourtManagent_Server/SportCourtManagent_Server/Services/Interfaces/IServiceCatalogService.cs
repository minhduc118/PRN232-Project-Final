using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Service;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IServiceCatalogService
    {
        Task<IEnumerable<ServiceDto>> GetAllAsync(bool activeOnly = false, string? category = null, string? search = null);
        Task<ServiceDto?> GetByIdAsync(int id);
        Task<ServiceDto> CreateAsync(CreateServiceRequest request);
        Task<ServiceDto?> UpdateAsync(int id, CreateServiceRequest request);
        Task<bool> DeactivateAsync(int id);
    }
}
