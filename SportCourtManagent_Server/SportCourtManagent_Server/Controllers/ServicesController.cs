using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Service;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceCatalogService _serviceCatalog;

        public ServicesController(IServiceCatalogService serviceCatalog)
        {
            _serviceCatalog = serviceCatalog ?? throw new ArgumentNullException(nameof(serviceCatalog));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? search, [FromQuery] bool activeOnly = true)
        {
            try
            {
                var result = await _serviceCatalog.GetAllAsync(activeOnly, category, search);
                return Ok(ApiResults.Ok(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });

            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _serviceCatalog.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy dịch vụ." });
                return Ok(ApiResults.Ok(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });

            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
        {
            try
            {
                var result = await _serviceCatalog.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.ServiceId },
                    ApiResults.Ok(result, "Tạo dịch vụ thành công.", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });

            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateServiceRequest request)
        {
            try
            {
                var result = await _serviceCatalog.UpdateAsync(id, request);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy dịch vụ." });
                return Ok(ApiResults.Ok(result, "Cập nhật dịch vụ thành công."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });

            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _serviceCatalog.DeactivateAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Không tìm thấy dịch vụ." });
                return Ok(ApiResults.Ok(null, "Vô hiệu hóa dịch vụ thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });

            }
        }
    }
}
