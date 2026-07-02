using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.ComplexService;
using SportCourtManagent_Server.Helpers;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/complexes/{complexId}/court-types/{courtTypeId}/services")]
    public class ComplexCourtTypeServicesController : ControllerBase
    {
        private readonly IComplexCourtTypeOfferingService _offeringService;

        public ComplexCourtTypeServicesController(IComplexCourtTypeOfferingService offeringService)
        {
            _offeringService = offeringService ?? throw new ArgumentNullException(nameof(offeringService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int complexId, int courtTypeId)
        {
            try
            {
                var result = await _offeringService.GetByComplexAndCourtTypeAsync(complexId, courtTypeId);
                return Ok(ApiResults.Ok(result));
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int complexId, int courtTypeId, [FromBody] CreateComplexCourtTypeServiceRequest request)
        {
            try
            {
                var result = await _offeringService.CreateAsync(complexId, courtTypeId, request);
                return CreatedAtAction(nameof(GetByOfferingId), new { complexId, courtTypeId, offeringId = result.OfferingId },
                    ApiResults.Ok(result, "Thêm dịch vụ cho loại sân thành công.", 201));
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

        [HttpGet("~/api/complexes/{complexId}/services")]
        public async Task<IActionResult> GetByComplex(int complexId)
        {
            try
            {
                var result = await _offeringService.GetByComplexAsync(complexId);
                return Ok(ApiResults.Ok(result));
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

        [HttpGet("~/api/complex-service-offerings/{offeringId}")]
        public async Task<IActionResult> GetByOfferingId(int offeringId)
        {
            try
            {
                var result = await _offeringService.GetByIdAsync(offeringId);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy cấu hình dịch vụ." });
                return Ok(ApiResults.Ok(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("~/api/complex-service-offerings/{offeringId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int offeringId, [FromBody] UpdateComplexCourtTypeServiceRequest request)
        {
            try
            {
                var result = await _offeringService.UpdateAsync(offeringId, request);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy cấu hình dịch vụ." });
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

        [HttpDelete("~/api/complex-service-offerings/{offeringId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int offeringId)
        {
            try
            {
                var deleted = await _offeringService.DeleteAsync(offeringId);
                if (!deleted)
                    return NotFound(new { message = "Không tìm thấy cấu hình dịch vụ." });
                return Ok(ApiResults.Ok(null, "Xóa dịch vụ khỏi loại sân thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
