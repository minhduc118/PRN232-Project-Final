using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SportCourtManagent_Server.Services.Implements
{
    public class CourtComplexService : ICourtComplexService
    {
        private static readonly string[] AllowedImageTypes =
            ["image/jpeg", "image/png", "image/webp", "image/gif"];

        private readonly ICourtComplexRepository _complexRepo;
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public CourtComplexService(ICourtComplexRepository complexRepo, IUserRepository userRepo, IConfiguration config)
        {
            _complexRepo = complexRepo ?? throw new ArgumentNullException(nameof(complexRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<PagedComplexResult> GetAllAsync(string? search, int? courtTypeId, int page, int pageSize)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);

            var allItems = await _complexRepo.GetAllWithDetailsAsync();
            var query = allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(cx =>
                    cx.ComplexName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    cx.Address.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (cx.Manager != null && cx.Manager.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            if (courtTypeId.HasValue)
            {
                query = query.Where(cx =>
                    cx.Courts.Any(c => !c.IsDeleted && c.CourtTypeId == courtTypeId.Value));
            }

            var filtered = query.ToList();
            var totalCount = filtered.Count;
            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            var stats = await _complexRepo.GetStatsAsync();

            return new PagedComplexResult
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Stats = stats
            };
        }

        public async Task<CourtComplexDto?> GetByIdAsync(int id)
        {
            var cx = await _complexRepo.GetByIdWithDetailsAsync(id);
            return cx == null ? null : MapToDto(cx);
        }

        public Task<ComplexStatsDto> GetStatsAsync() =>
            _complexRepo.GetStatsAsync();

        public async Task<CourtComplexDto> CreateAsync(UpsertCourtComplexRequest request)
        {
            ValidateRequest(request);

            var manager = await _userRepo.GetByIdWithDetailsAsync(request.ManagerId);
            if (manager == null || !manager.IsActive)
                throw new InvalidOperationException("Quản lý không tồn tại hoặc đã bị vô hiệu hóa.");
            if (!manager.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
                throw new InvalidOperationException("Người được chọn phải có vai trò Staff.");

            var complex = new CourtComplex
            {
                ComplexName = request.ComplexName.Trim(),
                Address = request.Address.Trim(),
                ManagerId = request.ManagerId,
                Description = request.Description?.Trim(),
                ImageUrl = request.ImageUrl?.Trim()
            };

            await _complexRepo.AddAsync(complex);
            return MapToDto(complex);
        }

        public async Task<CourtComplexDto?> UpdateAsync(int id, UpsertCourtComplexRequest request)
        {
            ValidateRequest(request);

            var complex = await _complexRepo.GetByIdWithDetailsAsync(id);
            if (complex == null) return null;

            var manager = await _userRepo.GetByIdWithDetailsAsync(request.ManagerId);
            if (manager == null || !manager.IsActive)
                throw new InvalidOperationException("Quản lý không tồn tại hoặc đã bị vô hiệu hóa.");
            if (!manager.UserRoles.Any(ur => ur.Role.RoleName == "Staff"))
                throw new InvalidOperationException("Người được chọn phải có vai trò Staff.");

            complex.ComplexName = request.ComplexName.Trim();
            complex.Address = request.Address.Trim();
            complex.ManagerId = request.ManagerId;
            complex.Description = request.Description?.Trim();
            complex.ImageUrl = request.ImageUrl?.Trim();
            complex.Manager = manager;

            await _complexRepo.UpdateAsync(complex);
            return MapToDto(complex);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var complex = await _complexRepo.GetByIdWithDetailsAsync(id);
            if (complex == null) return false;

            if (complex.Courts.Any(c => !c.IsDeleted))
                throw new InvalidOperationException("Vui lòng xóa hết sân trước khi xóa tổ hợp.");

            await _complexRepo.SoftDeleteAsync(id);
            return true;
        }

        public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string scheme, string host)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("Vui lòng chọn ảnh.");

            if (!AllowedImageTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Chỉ hỗ trợ ảnh JPG, PNG, WEBP, GIF.");

            var cloudName = _config["CloudinarySettings:CloudName"];
            var apiKey = _config["CloudinarySettings:ApiKey"];
            var apiSecret = _config["CloudinarySettings:ApiSecret"];

            // Nếu người dùng chưa điền token Cloudinary thực tế (vẫn để placeholder), 
            // tự động fallback về lưu local disk để tránh lỗi crash
            if (string.IsNullOrWhiteSpace(cloudName) || cloudName.Contains("YOUR_CLOUD_NAME") ||
                string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("YOUR_API_KEY") ||
                string.IsNullOrWhiteSpace(apiSecret) || apiSecret.Contains("YOUR_API_SECRET"))
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "complexes");
                Directory.CreateDirectory(uploadsDir);

                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                await using (var stream = File.Create(filePath))
                    await file.CopyToAsync(stream);

                return new ImageUploadResultDto
                {
                    Url = $"{scheme}://{host}/uploads/complexes/{fileName}"
                };
            }

            // Tiến hành upload lên Cloudinary
            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account);
            var uploadResult = new ImageUploadResult();

            await using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "sportcourt",
                    PublicId = $"{Guid.NewGuid():N}"
                };
                uploadResult = await cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult.Error != null)
                throw new InvalidOperationException($"Lỗi upload Cloudinary: {uploadResult.Error.Message}");

            return new ImageUploadResultDto
            {
                Url = uploadResult.SecureUrl.ToString()
            };
        }

        // ─── Private helpers ────────────────────────────────────────────────────

        private static void ValidateRequest(UpsertCourtComplexRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ComplexName))
                throw new ArgumentException("Tên tổ hợp không được để trống.");
            if (string.IsNullOrWhiteSpace(request.Address))
                throw new ArgumentException("Địa chỉ không được để trống.");
            if (request.ManagerId <= 0)
                throw new ArgumentException("Vui lòng chọn quản lý phụ trách.");
        }

        private static CourtComplexDto MapToDto(CourtComplex cx) => new()
        {
            ComplexId = cx.ComplexId,
            ComplexName = cx.ComplexName,
            Address = cx.Address,
            Phone = cx.Manager?.Phone,
            ManagerName = cx.Manager?.FullName,
            ManagerId = cx.ManagerId,
            Description = cx.Description,
            ImageUrl = cx.ImageUrl,
            TotalCourts = cx.Courts.Count(c => !c.IsDeleted),
            ActiveCourts = cx.Courts.Count(c => !c.IsDeleted && c.Status == CourtStatus.Available),
            MaintenanceCourts = cx.Courts.Count(c => !c.IsDeleted && c.Status == CourtStatus.Maintenance),
            InactiveCourts = cx.Courts.Count(c => !c.IsDeleted && c.Status == CourtStatus.Inactive),
            CourtTypeIds = cx.Courts.Where(c => !c.IsDeleted).Select(c => c.CourtTypeId).Distinct().ToList(),
            CreatedAt = cx.CreatedAt
        };
    }
}
