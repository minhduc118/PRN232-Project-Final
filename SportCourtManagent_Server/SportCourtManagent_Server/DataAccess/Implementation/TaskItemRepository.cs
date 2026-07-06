using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly AppDbContext _context;

        public TaskItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<TaskItem> Items, int TotalCount)> GetTasksByComplexAsync(
            int complexId,
            TaskItemStatus? status = null,
            TaskPriority? priority = null,
            int? assignedStaffId = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Tasks
                .Include(t => t.Complex)
                .Include(t => t.AssignedStaff)
                .Include(t => t.CreatedBy)
                .Where(t => t.ComplexId == complexId)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            if (assignedStaffId.HasValue)
            {
                query = query.Where(t => t.AssignedStaffId == assignedStaffId.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Complex)
                .Include(t => t.AssignedStaff)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.TaskId == id);
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> UpdateAsync(TaskItem task)
        {
            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;
            _context.Tasks.Remove(task);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
