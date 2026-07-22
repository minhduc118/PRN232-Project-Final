using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Service;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            "Equipment", "Drink", "Coach", "Event"
        };

        private readonly IServiceRepository _serviceRepo;

        public ServiceCatalogService(IServiceRepository serviceRepo)
        {
            _serviceRepo = serviceRepo ?? throw new ArgumentNullException(nameof(serviceRepo));
        }

        public async Task<IEnumerable<ServiceDto>> GetAllAsync(bool activeOnly = false, string? category = null, string? search = null)
        {
            var services = await _serviceRepo.GetAllAsync(activeOnly);
            var query = services.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s =>
                    s.ServiceName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (s.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            return query.Select(MapToDto).ToList();
        }

        public async Task<ServiceDto?> GetByIdAsync(int id)
        {
            var service = await _serviceRepo.GetByIdAsync(id);
            return service == null ? null : MapToDto(service);
        }

        public async Task<ServiceDto> CreateAsync(CreateServiceRequest request)
        {
            ValidateCategory(request.Category);
            if (await _serviceRepo.ExistsByNameAsync(request.ServiceName))
                throw new InvalidOperationException("Tên dịch vụ đã tồn tại.");

            var service = new Service
            {
                ServiceName = request.ServiceName.Trim(),
                Category = request.Category,
                Price = request.Price,
                Unit = string.IsNullOrWhiteSpace(request.Unit) ? "cái" : request.Unit.Trim(),
                Description = request.Description?.Trim(),
                StockQty = request.StockQty,
                IsActive = request.IsActive
            };

            await _serviceRepo.AddAsync(service);
            return MapToDto(service);
        }

        public async Task<ServiceDto?> UpdateAsync(int id, CreateServiceRequest request)
        {
            ValidateCategory(request.Category);
            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null) return null;

            if (await _serviceRepo.ExistsByNameAsync(request.ServiceName, id))
                throw new InvalidOperationException("Tên dịch vụ đã tồn tại.");

            service.ServiceName = request.ServiceName.Trim();
            service.Category = request.Category;
            service.Price = request.Price;
            service.Unit = string.IsNullOrWhiteSpace(request.Unit) ? "cái" : request.Unit.Trim();
            service.Description = request.Description?.Trim();
            service.StockQty = request.StockQty;
            service.IsActive = request.IsActive;

            await _serviceRepo.UpdateAsync(service);
            return MapToDto(service);
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null) return false;
            await _serviceRepo.DeleteAsync(id);
            return true;
        }

        private static void ValidateCategory(string category)
        {
            if (!AllowedCategories.Contains(category))
                throw new InvalidOperationException("Loại dịch vụ không hợp lệ. Chọn: Equipment, Drink, Coach, Event.");
        }

        private static ServiceDto MapToDto(Service s) => new()
        {
            ServiceId = s.ServiceId,
            ServiceName = s.ServiceName,
            Category = s.Category,
            Price = s.Price,
            Unit = s.Unit,
            Description = s.Description,
            StockQty = s.StockQty,
            IsActive = s.IsActive
        };
    }
}
