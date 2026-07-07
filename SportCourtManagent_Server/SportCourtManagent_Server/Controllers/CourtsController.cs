using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/courts")]
    public class CourtsController : ControllerBase
    {
        private readonly ICourtService _courtService;

        public CourtsController(ICourtService courtService)
        {
            _courtService = courtService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? complexId, [FromQuery] string? status)
        {
            var courts = await _courtService.GetAllAsync(complexId, status);
            return Ok(ApiResults.Ok(courts, "Lấy danh sách sân thành công."));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var court = await _courtService.GetByIdAsync(id);
            if (court == null)
                return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));
            return Ok(ApiResults.Ok(court, "Lấy thông tin sân thành công."));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CourtDto request)
        {
            try
            {
                if (await _courtService.ExistsByCodeAsync(request.CourtCode))
                {
                    return Conflict(ApiResults.Fail("Mã sân đã tồn tại trong hệ thống. Vui lòng chọn mã khác.", 409));
                }

                var result = await _courtService.CreateAsync(request);
                return StatusCode(201, ApiResults.Ok(result, "Tạo sân thể thao thành công.", 201));
            }
            catch (System.ArgumentException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
            catch (System.InvalidOperationException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CourtDto request)
        {
            try
            {
                if (await _courtService.ExistsByCodeAsync(request.CourtCode, id))
                {
                    return Conflict(ApiResults.Fail("Mã sân đã tồn tại trong hệ thống. Vui lòng chọn mã khác.", 409));
                }

                await _courtService.UpdateAsync(id, request);
                return Ok(ApiResults.Ok(null, "Cập nhật sân thể thao thành công."));
            }
            catch (System.Collections.Generic.KeyNotFoundException) { return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404)); }
            catch (System.ArgumentException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
            catch (System.InvalidOperationException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var court = await _courtService.GetByIdAsync(id);
                if (court == null)
                    return NotFound(ApiResults.Fail("Không tìm thấy sân.", 404));

                await _courtService.DeleteAsync(id);
                return Ok(ApiResults.Ok(null, "Xóa sân thành công."));
            }
            catch (System.InvalidOperationException ex) { return BadRequest(ApiResults.Fail(ex.Message)); }
        }
    }
}
