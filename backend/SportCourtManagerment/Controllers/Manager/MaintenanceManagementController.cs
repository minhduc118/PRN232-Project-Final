using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagerment.DTOs;
using SportCourtManagerment.DTOs.Maintenance;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Services.Interface;

namespace SportCourtManagerment.Controllers.Manager;

[ApiController]
[Route("api/manager/complexes/{complexId:int}/maintenance")]
[Authorize]
public class MaintenanceManagementController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceManagementController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    // GET /api/manager/complexes/{complexId}/maintenance
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        int complexId,
        [FromQuery] MaintenanceStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _maintenanceService.GetTasksAsync(complexId, status, page, pageSize);
            return Ok(ApiResponse<PagedMaintenanceResponse>.Ok(result, "Lấy danh sách công việc bảo trì thành công."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // GET /api/manager/complexes/{complexId}/maintenance/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int complexId, int id)
    {
        try
        {
            var result = await _maintenanceService.GetByIdAsync(id);
            if (result.ComplexId != complexId)
                return Forbid();

            return Ok(ApiResponse<MaintenanceResponse>.Ok(result, "Lấy chi tiết công việc bảo trì thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // POST /api/manager/complexes/{complexId}/maintenance
    [HttpPost]
    public async Task<IActionResult> Create(int complexId, [FromBody] CreateMaintenanceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                "Dữ liệu đầu vào không hợp lệ.",
                errors: ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        try
        {
            var result = await _maintenanceService.CreateTaskAsync(complexId, request);
            return CreatedAtAction(
                nameof(GetById),
                new { complexId, id = result.MaintenanceId },
                ApiResponse<MaintenanceResponse>.Created(result, "Tạo công việc bảo trì thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // PUT /api/manager/complexes/{complexId}/maintenance/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int complexId, int id, [FromBody] UpdateMaintenanceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ."));

        try
        {
            var result = await _maintenanceService.UpdateTaskAsync(complexId, id, request);
            return Ok(ApiResponse<MaintenanceResponse>.Ok(result, "Cập nhật công việc bảo trì thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // PUT /api/manager/complexes/{complexId}/maintenance/{id}/verify
    [HttpPut("{id:int}/verify")]
    public async Task<IActionResult> Verify(int complexId, int id, [FromBody] VerifyMaintenanceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ."));

        try
        {
            var result = await _maintenanceService.VerifyTaskAsync(complexId, id, request);
            var msg = request.IsApproved ? "Nhiệm thu công việc thành công." : "Đã từ chối nghiệm thu công việc.";
            return Ok(ApiResponse<MaintenanceResponse>.Ok(result, msg));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    // DELETE /api/manager/complexes/{complexId}/maintenance/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int complexId, int id)
    {
        try
        {
            await _maintenanceService.DeleteTaskAsync(complexId, id);
            return Ok(ApiResponse<object>.Ok(null, "Xóa công việc bảo trì thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }
}
