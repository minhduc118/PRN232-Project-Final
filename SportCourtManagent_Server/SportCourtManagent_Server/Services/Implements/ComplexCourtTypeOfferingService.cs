using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.ComplexService;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class ComplexCourtTypeOfferingService : IComplexCourtTypeOfferingService
    {
        private readonly IComplexCourtTypeServiceRepository _offeringRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly AppDbContext _context;

        public ComplexCourtTypeOfferingService(
            IComplexCourtTypeServiceRepository offeringRepo,
            IServiceRepository serviceRepo,
            AppDbContext context)
        {
            _offeringRepo = offeringRepo ?? throw new ArgumentNullException(nameof(offeringRepo));
            _serviceRepo = serviceRepo ?? throw new ArgumentNullException(nameof(serviceRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ComplexCourtTypeServiceDto>> GetByComplexAndCourtTypeAsync(int complexId, int courtTypeId)
        {
            await EnsureComplexAndCourtTypeExistAsync(complexId, courtTypeId);
            var items = await _offeringRepo.GetByComplexAndCourtTypeAsync(complexId, courtTypeId);
            return items.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<ComplexCourtTypeServiceDto>> GetByComplexAsync(int complexId)
        {
            await EnsureComplexExistsAsync(complexId);
            var items = await _offeringRepo.GetByComplexAsync(complexId);
            return items.Select(MapToDto).ToList();
        }

        public async Task<ComplexCourtTypeServiceDto?> GetByIdAsync(int offeringId)
        {
            var item = await _offeringRepo.GetByIdAsync(offeringId);
            return item == null ? null : MapToDto(item);
        }

        public async Task<ComplexCourtTypeServiceDto> CreateAsync(int complexId, int courtTypeId, CreateComplexCourtTypeServiceRequest request)
        {
            await EnsureComplexAndCourtTypeExistAsync(complexId, courtTypeId);

            var service = await _serviceRepo.GetByIdAsync(request.ServiceId)
                ?? throw new InvalidOperationException("Không tìm thấy dịch vụ trong danh mục.");

            if (!service.IsActive)
                throw new InvalidOperationException("Dịch vụ đã bị vô hiệu hóa trong danh mục.");

            if (await _offeringRepo.ExistsAsync(complexId, courtTypeId, request.ServiceId))
                throw new InvalidOperationException("Dịch vụ này đã được gán cho loại sân tại tổ hợp.");

            var price = ResolvePrice(request.ServiceMode, request.Price, service.Price);

            var offering = new ComplexCourtTypeService
            {
                ComplexId = complexId,
                CourtTypeId = courtTypeId,
                ServiceId = request.ServiceId,
                Price = price,
                StockQty = request.StockQty,
                ServiceMode = request.ServiceMode,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _offeringRepo.AddAsync(offering);
            var created = await _offeringRepo.GetByIdAsync(offering.OfferingId);
            return MapToDto(created!);
        }

        public async Task<ComplexCourtTypeServiceDto?> UpdateAsync(int offeringId, UpdateComplexCourtTypeServiceRequest request)
        {
            var offering = await _offeringRepo.GetByIdAsync(offeringId);
            if (offering == null) return null;

            offering.Price = ResolvePrice(request.ServiceMode, request.Price, offering.Service.Price);
            offering.StockQty = request.StockQty;
            offering.ServiceMode = request.ServiceMode;
            offering.IsActive = request.IsActive;

            await _offeringRepo.UpdateAsync(offering);
            var updated = await _offeringRepo.GetByIdAsync(offeringId);
            return updated == null ? null : MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int offeringId)
        {
            var offering = await _offeringRepo.GetByIdAsync(offeringId);
            if (offering == null) return false;
            await _offeringRepo.DeleteAsync(offeringId);
            return true;
        }

        private static decimal ResolvePrice(ServiceMode mode, decimal requestedPrice, decimal defaultPrice)
        {
            if (mode == ServiceMode.Included)
                return 0m;
            return requestedPrice > 0 ? requestedPrice : defaultPrice;
        }

        private async Task EnsureComplexExistsAsync(int complexId)
        {
            var exists = await _context.CourtComplexes.AnyAsync(c => c.ComplexId == complexId && !c.IsDeleted);
            if (!exists)
                throw new InvalidOperationException("Không tìm thấy tổ hợp sân.");
        }

        private async Task EnsureComplexAndCourtTypeExistAsync(int complexId, int courtTypeId)
        {
            await EnsureComplexExistsAsync(complexId);

            var courtTypeExists = await _context.CourtTypes.AnyAsync(ct => ct.CourtTypeId == courtTypeId && ct.IsActive);
            if (!courtTypeExists)
                throw new InvalidOperationException("Không tìm thấy loại sân.");

            var hasCourtTypeInComplex = await _context.Courts.AnyAsync(c =>
                c.ComplexId == complexId && c.CourtTypeId == courtTypeId && !c.IsDeleted);
            if (!hasCourtTypeInComplex)
                throw new InvalidOperationException("Tổ hợp này chưa có sân thuộc loại sân đã chọn.");
        }

        private static ComplexCourtTypeServiceDto MapToDto(ComplexCourtTypeService o) => new()
        {
            OfferingId = o.OfferingId,
            ComplexId = o.ComplexId,
            CourtTypeId = o.CourtTypeId,
            CourtTypeName = o.CourtType?.TypeName ?? string.Empty,
            ServiceId = o.ServiceId,
            ServiceName = o.Service?.ServiceName ?? string.Empty,
            Category = o.Service?.Category ?? string.Empty,
            Unit = o.Service?.Unit ?? string.Empty,
            Price = o.Price,
            StockQty = o.StockQty,
            ServiceMode = o.ServiceMode.ToString(),
            IsActive = o.IsActive
        };
    }
}
