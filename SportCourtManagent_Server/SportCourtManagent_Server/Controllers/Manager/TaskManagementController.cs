using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Task;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Controllers.Manager
{
    [Authorize(Roles = "Admin,Manager")]
    [ApiController]
    [Route("api/manager/complexes/{complexId:int}/tasks")]
    public class TaskManagementController : ControllerBase
    {
        private readonly ITaskItemService _taskService;

        public TaskManagementController(ITaskItemService taskService)
        {
            _taskService = taskService;
        }

        // GET /api/manager/complexes/{complexId}/tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromRoute] int complexId,
            [FromQuery] TaskItemStatus? status = null,
            [FromQuery] TaskPriority? priority = null,
            [FromQuery] int? assignedStaffId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _taskService.GetTasksByComplexAsync(complexId, status, priority, assignedStaffId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // GET /api/manager/complexes/{complexId}/tasks/{taskId}
        [HttpGet("{taskId:int}")]
        public async Task<IActionResult> GetTaskById([FromRoute] int complexId, [FromRoute] int taskId)
        {
            try
            {
                var result = await _taskService.GetTaskByIdAsync(complexId, taskId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // POST /api/manager/complexes/{complexId}/tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromRoute] int complexId, [FromBody] CreateTaskRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetUserId(out int managerId))
            {
                return Unauthorized(new { Message = "Không xác định được người dùng." });
            }

            try
            {
                var result = await _taskService.CreateTaskAsync(complexId, managerId, request);
                return CreatedAtAction(nameof(GetTaskById), new { complexId = complexId, taskId = result.TaskId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // PUT /api/manager/complexes/{complexId}/tasks/{taskId}
        [HttpPut("{taskId:int}")]
        public async Task<IActionResult> UpdateTask(
            [FromRoute] int complexId,
            [FromRoute] int taskId,
            [FromBody] UpdateTaskRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _taskService.UpdateTaskAsync(complexId, taskId, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // PUT /api/manager/complexes/{complexId}/tasks/{taskId}/verify
        [HttpPut("{taskId:int}/verify")]
        public async Task<IActionResult> VerifyTask(
            [FromRoute] int complexId,
            [FromRoute] int taskId,
            [FromBody] VerifyTaskRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _taskService.VerifyTaskAsync(complexId, taskId, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // DELETE /api/manager/complexes/{complexId}/tasks/{taskId}
        [HttpDelete("{taskId:int}")]
        public async Task<IActionResult> DeleteTask([FromRoute] int complexId, [FromRoute] int taskId)
        {
            try
            {
                await _taskService.DeleteTaskAsync(complexId, taskId);
                return Ok(new { Message = "Xóa công việc thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(claim) && int.TryParse(claim, out userId);
        }
    }
}
