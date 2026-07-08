using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Task;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Controllers
{
    [Authorize(Roles = "Staff")]
    [ApiController]
    [Route("api/staff/tasks")]
    public class StaffTasksController : ControllerBase
    {
        private readonly ITaskItemService _taskService;

        public StaffTasksController(ITaskItemService taskService)
        {
            _taskService = taskService;
        }

        // GET /api/staff/tasks
        [HttpGet]
        public async Task<IActionResult> GetMyTasks(
            [FromQuery] TaskItemStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetUserId(out int staffId))
            {
                return Unauthorized(new { Message = "Không xác định được người dùng." });
            }

            try
            {
                var result = await _taskService.GetStaffTasksAsync(staffId, status, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // PUT /api/staff/tasks/{taskId}/start
        [HttpPut("{taskId:int}/start")]
        public async Task<IActionResult> StartTask([FromRoute] int taskId)
        {
            if (!TryGetUserId(out int staffId))
            {
                return Unauthorized(new { Message = "Không xác định được người dùng." });
            }

            try
            {
                var result = await _taskService.StartTaskAsync(staffId, taskId);
                return Ok(result);
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

        // PUT /api/staff/tasks/{taskId}/complete
        [HttpPut("{taskId:int}/complete")]
        public async Task<IActionResult> CompleteTask([FromRoute] int taskId)
        {
            if (!TryGetUserId(out int staffId))
            {
                return Unauthorized(new { Message = "Không xác định được người dùng." });
            }

            try
            {
                var result = await _taskService.CompleteTaskAsync(staffId, taskId);
                return Ok(result);
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
