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
