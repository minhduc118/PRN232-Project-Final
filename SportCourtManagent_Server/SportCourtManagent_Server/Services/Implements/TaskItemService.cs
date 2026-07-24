using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Task;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Services.Implements
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ITaskItemRepository _taskRepository;
        private readonly ICourtComplexRepository _complexRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly INotificationRepository _notificationRepository;

        public TaskItemService(
            ITaskItemRepository taskRepository,
            ICourtComplexRepository complexRepository,
            IStaffRepository staffRepository,
            IBookingRepository bookingRepository,
            INotificationRepository notificationRepository)
        {
            _taskRepository = taskRepository;
            _complexRepository = complexRepository;
            _staffRepository = staffRepository;
            _bookingRepository = bookingRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<PagedTaskResponse> GetTasksByComplexAsync(
            int complexId,
            TaskItemStatus? status,
            TaskPriority? priority,
            int? assignedStaffId,
            int page,
            int pageSize)
        {
            var (items, totalCount) = await _taskRepository.GetTasksByComplexAsync(
                complexId, status, priority, assignedStaffId, page, pageSize);

            var list = new List<TaskResponse>();
            foreach (var item in items)
            {
                list.Add(MapToResponse(item));
            }

            return new PagedTaskResponse
            {
                Items = list,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<TaskResponse> GetTaskByIdAsync(int complexId, int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy công việc với Id {taskId} thuộc cơ sở {complexId}.");
            }
            return MapToResponse(task);
        }

        public async Task<TaskResponse> CreateTaskAsync(int complexId, int managerId, CreateTaskRequest request)
        {
            if (!request.Category.HasValue || !request.Priority.HasValue || !request.DueDate.HasValue || !request.AssignedStaffId.HasValue)
            {
                throw new ArgumentException("Thiếu các trường bắt buộc (Danh mục, Độ ưu tiên, Hạn hoàn thành hoặc Nhân viên thực hiện).");
            }

            if (request.DueDate.Value <= GetVietnamTime(DateTime.UtcNow))
            {
                throw new ArgumentException("Hạn hoàn thành phải ở trong tương lai.");
            }

            var complex = await _complexRepository.GetByIdWithDetailsAsync(complexId);
            if (complex == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cơ sở với Id {complexId}.");
            }

            if (request.AssignedStaffId.HasValue)
            {
                var isStaff = await _staffRepository.IsStaffOfComplexAsync(request.AssignedStaffId.Value, complexId);
                if (!isStaff)
                {
                    throw new ArgumentException("Nhân viên được gán không thuộc cơ sở này.");
                }
            }

            if (request.BookingId.HasValue)
            {
                var booking = await _bookingRepository.GetDetailAsync(request.BookingId.Value);
                if (booking == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy đơn đặt sân với Id {request.BookingId.Value}.");
                }
            }

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                TaskType = TaskType.Manual,
                Category = request.Category.Value,
                Priority = request.Priority.Value,
                Status = TaskItemStatus.Pending,
                ComplexId = complexId,
                AssignedStaffId = request.AssignedStaffId,
                CreatedById = managerId,
                BookingId = request.BookingId,
                DueDate = request.DueDate.Value,
                CreatedAt = DateTime.Now
            };

            var created = await _taskRepository.CreateAsync(task);
            var loadedTask = await _taskRepository.GetByIdAsync(created.TaskId);

            if (loadedTask != null && loadedTask.AssignedStaffId.HasValue)
            {
                await _notificationRepository.CreateNotificationAsync(new Notification
                {
                    UserId = loadedTask.AssignedStaffId.Value,
                    Title = $"Bạn được phân công công việc mới: {loadedTask.Title}",
                    Type = NotificationType.System,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }

            return MapToResponse(loadedTask ?? created);
        }

        public async Task<TaskResponse> UpdateTaskAsync(int complexId, int taskId, UpdateTaskRequest request)
        {
            if (!request.Category.HasValue || !request.Priority.HasValue || !request.DueDate.HasValue)
            {
                throw new ArgumentException("Thiếu các trường bắt buộc.");
            }

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy công việc với Id {taskId} thuộc cơ sở {complexId}.");
            }

            int? oldStaffId = task.AssignedStaffId;

            if (request.AssignedStaffId.HasValue && request.AssignedStaffId != oldStaffId)
            {
                var isStaff = await _staffRepository.IsStaffOfComplexAsync(request.AssignedStaffId.Value, complexId);
                if (!isStaff)
                {
                    throw new ArgumentException("Nhân viên được gán không thuộc cơ sở này.");
                }
            }

            if (request.BookingId.HasValue)
            {
                var booking = await _bookingRepository.GetDetailAsync(request.BookingId.Value);
                if (booking == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy đơn đặt sân với Id {request.BookingId.Value}.");
                }
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Category = request.Category.Value;
            task.Priority = request.Priority.Value;
            task.AssignedStaffId = request.AssignedStaffId;
            task.BookingId = request.BookingId;
            task.DueDate = request.DueDate.Value;

            var updated = await _taskRepository.UpdateAsync(task);
            var loadedTask = await _taskRepository.GetByIdAsync(updated.TaskId) ?? updated;

            if (request.AssignedStaffId != oldStaffId)
            {
                if (oldStaffId.HasValue)
                {
                    await _notificationRepository.CreateNotificationAsync(new Notification
                    {
                        UserId = oldStaffId.Value,
                        Title = $"Công việc đã được thu hồi hoặc gán cho người khác: {loadedTask.Title}",
                        Type = NotificationType.System,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
                if (request.AssignedStaffId.HasValue)
                {
                    await _notificationRepository.CreateNotificationAsync(new Notification
                    {
                        UserId = request.AssignedStaffId.Value,
                        Title = $"Bạn được phân công công việc mới: {loadedTask.Title}",
                        Type = NotificationType.System,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            return MapToResponse(loadedTask);
        }

        public async Task<TaskResponse> VerifyTaskAsync(int complexId, int taskId, VerifyTaskRequest request)
        {
            if (!request.IsApproved.HasValue)
            {
                throw new ArgumentException("Thiếu trường IsApproved.");
            }

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy công việc với Id {taskId} thuộc cơ sở {complexId}.");
            }

            if (request.IsApproved.Value)
            {
                task.Status = TaskItemStatus.Approved;
                task.CompletedAt = DateTime.Now;
                if (!string.IsNullOrEmpty(request.Note))
                {
                    task.Description = $"{task.Description}. Kết quả duyệt: {request.Note}";
                }
            }
            else
            {
                task.Status = TaskItemStatus.InProgress;
                task.CompletedAt = null;
                if (!string.IsNullOrEmpty(request.Note))
                {
                    task.Description = $"{task.Description}. Lý do từ chối: {request.Note}";
                }
            }

            var updated = await _taskRepository.UpdateAsync(task);
            var loadedTask = await _taskRepository.GetByIdAsync(updated.TaskId) ?? updated;

            if (loadedTask.AssignedStaffId.HasValue)
            {
                string statusText = request.IsApproved.Value ? "đã được duyệt hoàn thành" : "bị từ chối và yêu cầu làm lại";
                await _notificationRepository.CreateNotificationAsync(new Notification
                {
                    UserId = loadedTask.AssignedStaffId.Value,
                    Title = $"Kết quả nghiệm thu công việc '{loadedTask.Title}': {statusText}",
                    Type = NotificationType.System,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }

            return MapToResponse(loadedTask);
        }

        public async Task DeleteTaskAsync(int complexId, int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy công việc với Id {taskId} thuộc cơ sở {complexId}.");
            }

            if (task.Status == TaskItemStatus.Approved)
            {
                throw new InvalidOperationException("Không cho phép xóa công việc đã được nghiệm thu hoàn thành.");
            }

            await _taskRepository.DeleteAsync(task.TaskId);
        }

        public async Task<PagedTaskResponse> GetStaffTasksAsync(
            int staffId,
            TaskItemStatus? status,
            int page,
            int pageSize)
        {
            var (items, totalCount) = await _taskRepository.GetTasksByStaffAsync(
                staffId, status, page, pageSize);

            var list = new List<TaskResponse>();
            foreach (var item in items)
            {
                list.Add(MapToResponse(item));
            }

            return new PagedTaskResponse
            {
                Items = list,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<TaskResponse> StartTaskAsync(int staffId, int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.AssignedStaffId != staffId)
            {
                throw new KeyNotFoundException($"Không tìm thấy công việc với Id {taskId} giao cho bạn.");
            }

            if (task.Status != TaskItemStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ có thể bắt đầu công việc đang ở trạng thái Chờ thực hiện (Pending).");
            }

            task.Status = TaskItemStatus.InProgress;
            var updated = await _taskRepository.UpdateAsync(task);
            var loadedTask = await _taskRepository.GetByIdAsync(updated.TaskId) ?? updated;
            return MapToResponse(loadedTask);
        }

        public async Task<TaskResponse> CompleteTaskAsync(int staffId, int taskId, CompleteTaskRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ResultNote))
            {
                throw new ArgumentException("Vui lòng nhập mô tả kết quả để hoàn thành công việc.");
            }


            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.AssignedStaffId != staffId)
            {
                throw new KeyNotFoundException($"Không tìm thấy công việc với Id {taskId} giao cho bạn.");
            }

            if (task.Status != TaskItemStatus.InProgress)
            {
                throw new InvalidOperationException("Chỉ có thể hoàn thành công việc đang ở trạng thái Đang làm (InProgress).");
            }

            task.Status = TaskItemStatus.Completed;
            task.ImageProof = request.ProofImageUrl;
            task.Description = string.IsNullOrWhiteSpace(task.Description)
                ? $"[Mô tả kết quả]: {request.ResultNote}"
                : $"{task.Description}\n\n[Mô tả kết quả hoàn thành]: {request.ResultNote}";

            var updated = await _taskRepository.UpdateAsync(task);
            var loadedTask = await _taskRepository.GetByIdAsync(updated.TaskId) ?? updated;
            return MapToResponse(loadedTask);
        }


        private TaskResponse MapToResponse(TaskItem item)
        {
            return new TaskResponse
            {
                TaskId = item.TaskId,
                Title = item.Title,
                Description = item.Description,
                TaskType = item.TaskType.ToString(),
                Category = item.Category.ToString(),
                Priority = item.Priority.ToString(),
                Status = item.Status.ToString(),
                ComplexId = item.ComplexId,
                AssignedStaffId = item.AssignedStaffId,
                AssignedStaffName = item.AssignedStaff?.FullName,
                CreatedById = item.CreatedById,
                CreatedByName = item.CreatedBy?.FullName,
                BookingId = item.BookingId,
                DueDate = item.DueDate,
                CreatedAt = item.CreatedAt,
                CompletedAt = item.CompletedAt,
                ImageProof = item.ImageProof
            };
        }

        private DateTime GetVietnamTime(DateTime utcNow)
        {
            TimeZoneInfo vnZone;
            try
            {
                vnZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                vnZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnZone);
        }
    }
}
