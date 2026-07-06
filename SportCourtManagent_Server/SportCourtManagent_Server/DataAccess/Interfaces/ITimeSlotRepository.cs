using System.Collections.Generic;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ITimeSlotRepository
    {
        Task<IEnumerable<TimeSlot>> GetAllAsync();
        Task<TimeSlot?> GetByIdAsync(int id);
        Task AddAsync(TimeSlot entity);
        Task UpdateAsync(TimeSlot entity);
        Task DeleteAsync(int id);
    }
}
