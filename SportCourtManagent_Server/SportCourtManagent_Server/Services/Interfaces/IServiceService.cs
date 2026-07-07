using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Services;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetAllServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task<Service> CreateServiceAsync(ServiceRequestDto dto);
        Task<bool> UpdateServiceAsync(int id, ServiceRequestDto dto);
        Task<bool> DeleteServiceAsync(int id);
    }
}
