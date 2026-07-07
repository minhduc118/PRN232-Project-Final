using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Services.Implements
{
    public class CourtTypeService : ICourtTypeService
    {
        private readonly ICourtTypeRepository _courtTypeRepo;

        public CourtTypeService(ICourtTypeRepository courtTypeRepo)
        {
            _courtTypeRepo = courtTypeRepo ?? throw new ArgumentNullException(nameof(courtTypeRepo));
        }

        public async Task<IEnumerable<CourtTypeDto>> GetAllActiveAsync()
        {
            var types = await _courtTypeRepo.GetAllActiveAsync();
            return types.Select(t => new CourtTypeDto
            {
                CourtTypeId = t.CourtTypeId,
                TypeName = t.TypeName,
                IsActive = t.IsActive
            }).ToList();
        }
    }
}
