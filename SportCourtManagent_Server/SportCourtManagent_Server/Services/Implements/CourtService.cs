using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class CourtService : ICourtService
    {
        private readonly ICourtRepository _courtRepo;

        public CourtService(ICourtRepository courtRepo)
        {
            _courtRepo = courtRepo ?? throw new ArgumentNullException(nameof(courtRepo));
        }

        public async Task<IEnumerable<CourtDto>> GetAllAsync(int? complexId, string? status)
        {
            var courts = await _courtRepo.GetAllWithDetailsAsync(complexId, status);
            return courts.Select(MapToDto).ToList();
        }

        public async Task<CourtDto?> GetByIdAsync(int id)
        {
            var court = await _courtRepo.GetByIdWithDetailsAsync(id);
            return court == null ? null : MapToDto(court);
        }

        public async Task<CourtDto> CreateAsync(CourtDto dto)
        {
            if (!System.TimeSpan.TryParse(dto.OpenTime, out var openTime) ||
                !System.TimeSpan.TryParse(dto.CloseTime, out var closeTime))
            {
                throw new System.ArgumentException("Giờ mở/đóng cửa không đúng định dạng.");
            }

            if (!System.Enum.TryParse<SportCourtManagent_Server.Enums.CourtStatus>(dto.Status, true, out var status))
            {
                status = SportCourtManagent_Server.Enums.CourtStatus.Available;
            }

            var court = new Court
            {
                CourtName = dto.CourtName,
                CourtCode = dto.CourtCode,
                CourtTypeId = dto.CourtTypeId,
                ComplexId = dto.ComplexId,
                Status = status,
                OpenTime = openTime,
                CloseTime = closeTime,
                PricePerHour = dto.PricePerHour,
                CourtSize = dto.CourtSize,
                IsDeleted = false
            };

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                court.CourtImages.Add(new CourtImage
                {
                    ImageUrl = dto.ImageUrl,
                    IsPrimary = true
                });
            }

            await _courtRepo.AddAsync(court);

            var loaded = await _courtRepo.GetByIdWithDetailsAsync(court.CourtId);
            return loaded == null ? dto : MapToDto(loaded);
        }

        public async Task UpdateAsync(int id, CourtDto dto)
        {
            var court = await _courtRepo.GetByIdWithDetailsAsync(id);
            if (court == null) throw new System.Collections.Generic.KeyNotFoundException("Không tìm thấy sân.");

            if (!System.TimeSpan.TryParse(dto.OpenTime, out var openTime) ||
                !System.TimeSpan.TryParse(dto.CloseTime, out var closeTime))
            {
                throw new System.ArgumentException("Giờ mở/đóng cửa không đúng định dạng.");
            }

            if (!System.Enum.TryParse<SportCourtManagent_Server.Enums.CourtStatus>(dto.Status, true, out var status))
            {
                status = SportCourtManagent_Server.Enums.CourtStatus.Available;
            }

            court.CourtName = dto.CourtName;
            court.CourtCode = dto.CourtCode;
            court.CourtTypeId = dto.CourtTypeId;
            court.ComplexId = dto.ComplexId;
            court.Status = status;
            court.OpenTime = openTime;
            court.CloseTime = closeTime;
            court.PricePerHour = dto.PricePerHour;
            court.CourtSize = dto.CourtSize;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                var primary = court.CourtImages.FirstOrDefault(i => i.IsPrimary);
                if (primary != null)
                {
                    primary.ImageUrl = dto.ImageUrl;
                }
                else
                {
                    court.CourtImages.Add(new CourtImage
                    {
                        ImageUrl = dto.ImageUrl,
                        IsPrimary = true
                    });
                }
            }

            await _courtRepo.UpdateAsync(court);
        }

        public async Task DeleteAsync(int id)
        {
            await _courtRepo.SoftDeleteAsync(id);
        }

        public async Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null)
        {
            return await _courtRepo.ExistsByCodeAsync(courtCode, excludeCourtId);
        }

        private static CourtDto MapToDto(Court c) => new()
        {
            CourtId = c.CourtId,
            CourtName = c.CourtName,
            CourtCode = c.CourtCode,
            CourtTypeId = c.CourtTypeId,
            CourtTypeName = c.CourtType.TypeName,
            ComplexId = c.ComplexId,
            ComplexName = c.Complex.ComplexName,
            Status = c.Status.ToString(),
            OpenTime = c.OpenTime.ToString(@"hh\:mm"),
            CloseTime = c.CloseTime.ToString(@"hh\:mm"),
            PricePerHour = c.PricePerHour,
            CourtSize = c.CourtSize,
            ImageUrl = c.CourtImages.OrderBy(i => i.CourtImageId).Select(i => i.ImageUrl).FirstOrDefault()
        };
    }
}
