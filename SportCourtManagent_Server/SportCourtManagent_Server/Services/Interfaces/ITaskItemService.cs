using SportCourtManagent_Server.DTOs.Task;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Services.Interfaces
{
    public interface ITaskItemService
    {
        Task<PagedTaskResponse> GetTasksByComplexAsync(
            int complexId,
            TaskItemStatus? status,
            TaskPriority? priority,
            int? assignedStaffId,
            int page,
            int pageSize);

        Task<TaskResponse> GetTaskByIdAsync(int complexId, int taskId);
        Task<TaskResponse> CreateTaskAsync(int complexId, int managerId, CreateTaskRequest request);
        Task<TaskResponse> UpdateTaskAsync(int complexId, int taskId, UpdateTaskRequest request);
        Task<TaskResponse> VerifyTaskAsync(int complexId, int taskId, VerifyTaskRequest request);
        Task DeleteTaskAsync(int complexId, int taskId);
    }
}
