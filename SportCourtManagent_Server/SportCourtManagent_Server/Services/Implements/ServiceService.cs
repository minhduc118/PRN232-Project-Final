using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Services;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            return await _serviceRepository.GetAllAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _serviceRepository.GetByIdAsync(id);
        }

        public async Task<Service> CreateServiceAsync(ServiceRequestDto dto)
        {
            var service = new Service
            {
                ServiceName = dto.ServiceName,
                Category = dto.Category,
                Price = dto.Price,
                StockQty = dto.StockQty
            };

            await _serviceRepository.AddAsync(service);
            return service;
        }

        public async Task<bool> UpdateServiceAsync(int id, ServiceRequestDto dto)
        {
            var existingService = await _serviceRepository.GetByIdAsync(id);
            if (existingService == null)
            {
                return false;
            }

            existingService.ServiceName = dto.ServiceName;
            existingService.Category = dto.Category;
            existingService.Price = dto.Price;
            existingService.StockQty = dto.StockQty;

            await _serviceRepository.UpdateAsync(existingService);
            return true;
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var existingService = await _serviceRepository.GetByIdAsync(id);
            if (existingService == null)
            {
                return false;
            }

            await _serviceRepository.DeleteAsync(id);
            return true;
        }
    }
}
