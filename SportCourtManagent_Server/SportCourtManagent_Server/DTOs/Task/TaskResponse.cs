using System;
using System.Collections.Generic;

namespace SportCourtManagent_Server.DTOs.Task
{
    public class TaskResponse
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TaskType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ComplexId { get; set; }
        public int? AssignedStaffId { get; set; }
        public string? AssignedStaffName { get; set; }
        public int? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public int? BookingId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class PagedTaskResponse
    {
        public List<TaskResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
