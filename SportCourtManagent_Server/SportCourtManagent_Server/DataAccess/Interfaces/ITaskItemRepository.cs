using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface ITaskItemRepository
    {
        Task<(List<TaskItem> Items, int TotalCount)> GetTasksByComplexAsync(
            int complexId,
            TaskItemStatus? status = null,
            TaskPriority? priority = null,
            int? assignedStaffId = null,
            int page = 1,
            int pageSize = 10);

        Task<TaskItem?> GetByIdAsync(int id);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<TaskItem> UpdateAsync(TaskItem task);
        Task<bool> DeleteAsync(int id);
        Task<(List<TaskItem> Items, int TotalCount)> GetTasksByStaffAsync(
            int staffId,
            TaskItemStatus? status = null,
            int page = 1,
            int pageSize = 10);
    }
}
