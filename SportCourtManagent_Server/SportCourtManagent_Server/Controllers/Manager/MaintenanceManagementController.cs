using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Maintenance;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Controllers.Manager
{
    [Route("api/manager/complexes/{complexId:int}/maintenance")]
    [ApiController]
    public class MaintenanceManagementController : ControllerBase
    {
        private readonly IMaintenanceScheduleService _maintenanceService;

        public MaintenanceManagementController(IMaintenanceScheduleService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        // POST /api/manager/complexes/{complexId}/maintenance
        [HttpPost]
        public async Task<IActionResult> CreateMaintenance([FromRoute] int complexId, [FromBody] CreateMaintenanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _maintenanceService.CreateMaintenanceAsync(complexId, request);
                return CreatedAtAction(nameof(GetMaintenanceById), new { complexId = complexId, maintenanceId = result.MaintenanceId }, result);
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

        // GET /api/manager/complexes/{complexId}/maintenance
        [HttpGet]
        public async Task<IActionResult> GetMaintenanceList(
            [FromRoute] int complexId,
            [FromQuery] MaintenanceStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _maintenanceService.GetMaintenanceListAsync(complexId, status, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // GET /api/manager/complexes/{complexId}/maintenance/{maintenanceId}
        [HttpGet("{maintenanceId:int}")]
        public async Task<IActionResult> GetMaintenanceById([FromRoute] int complexId, [FromRoute] int maintenanceId)
        {
            try
            {
                var result = await _maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
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

        // PUT /api/manager/complexes/{complexId}/maintenance/{maintenanceId}
        [HttpPut("{maintenanceId:int}")]
        public async Task<IActionResult> UpdateMaintenance(
            [FromRoute] int complexId,
            [FromRoute] int maintenanceId,
            [FromBody] UpdateMaintenanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _maintenanceService.UpdateMaintenanceAsync(complexId, maintenanceId, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // DELETE /api/manager/complexes/{complexId}/maintenance/{maintenanceId}
        [HttpDelete("{maintenanceId:int}")]
        public async Task<IActionResult> DeleteMaintenance(int complexId, int maintenanceId)
        {
            try
            {
                await _maintenanceService.DeleteMaintenanceAsync(complexId, maintenanceId);
                return Ok(new { Message = "Xóa lịch bảo trì thành công." });
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

        // PUT /api/manager/complexes/{complexId}/maintenance/{maintenanceId}/verify
        [HttpPut("{maintenanceId:int}/verify")]
        public async Task<IActionResult> VerifyMaintenance(
            int complexId,
            int maintenanceId,
            [FromBody] VerifyMaintenanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _maintenanceService.VerifyMaintenanceAsync(complexId, maintenanceId, request);
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
    }
}
