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

        public Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            var services = _serviceRepository.GetAll();
            return Task.FromResult(services);
        }

        public Task<Service?> GetServiceByIdAsync(int id)
        {
            var service = _serviceRepository.GetById(id);
            return Task.FromResult(service);
        }

        public Task<Service> CreateServiceAsync(ServiceRequestDto dto)
        {
            var service = new Service
            {
                ServiceName = dto.ServiceName,
                Category = dto.Category,
                Price = dto.Price,
                StockQty = dto.StockQty
            };

            _serviceRepository.Add(service);
            return Task.FromResult(service);
        }

        public Task<bool> UpdateServiceAsync(int id, ServiceRequestDto dto)
        {
            var existingService = _serviceRepository.GetById(id);
            if (existingService == null)
            {
                return Task.FromResult(false);
            }

            existingService.ServiceName = dto.ServiceName;
            existingService.Category = dto.Category;
            existingService.Price = dto.Price;
            existingService.StockQty = dto.StockQty;

            _serviceRepository.Update(existingService);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteServiceAsync(int id)
        {
            var existingService = _serviceRepository.GetById(id);
            if (existingService == null)
            {
                return Task.FromResult(false);
            }

            _serviceRepository.Delete(id);
            return Task.FromResult(true);
        }
    }
}
