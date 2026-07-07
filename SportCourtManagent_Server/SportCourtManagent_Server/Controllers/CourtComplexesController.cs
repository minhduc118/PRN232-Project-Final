using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/complexes")]
    public class CourtComplexesController : ControllerBase
    {
        private readonly ICourtComplexService _complexService;

        public CourtComplexesController(ICourtComplexService complexService)
        {
            _complexService = complexService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int? courtTypeId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 8)
        {
            var result = await _complexService.GetAllAsync(search, courtTypeId, page, pageSize);
            return Ok(ApiResults.Ok(result, "Lấy danh sách tổ hợp sân thành công."));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _complexService.GetStatsAsync();
            return Ok(ApiResults.Ok(stats, "Lấy thống kê thành công."));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _complexService.GetByIdAsync(id);
            if (result == null)
                return NotFound(ApiResults.Fail("Không tìm thấy tổ hợp sân.", 404));
            return Ok(ApiResults.Ok(result, "Lấy thông tin tổ hợp sân thành công."));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] UpsertCourtComplexRequest request)
        {
            try
            {
                var result = await _complexService.CreateAsync(request);
                return StatusCode(201, ApiResults.Ok(result, "Tạo tổ hợp sân thành công.", 201));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertCourtComplexRequest request)
        {
            try
            {
                var result = await _complexService.UpdateAsync(id, request);
                if (result == null)
                    return NotFound(ApiResults.Fail("Không tìm thấy tổ hợp sân.", 404));
                return Ok(ApiResults.Ok(result, "Cập nhật tổ hợp sân thành công."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _complexService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(ApiResults.Fail("Không tìm thấy tổ hợp sân.", 404));
                return Ok(ApiResults.Ok(null, "Xóa tổ hợp sân thành công."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                var result = await _complexService.UploadImageAsync(
                    file, Request.Scheme, Request.Host.ToString());
                return Ok(ApiResults.Ok(result, "Upload ảnh thành công."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }
    }
}
