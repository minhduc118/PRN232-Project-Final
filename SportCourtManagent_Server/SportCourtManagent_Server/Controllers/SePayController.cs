using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Payments;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SePayController : ControllerBase
    {
        private readonly ISePayService _sePayService;

        public SePayController(ISePayService sePayService)
        {
            _sePayService = sePayService;
        }

        [HttpGet("qr-code/{bookingCode}")]
        public async Task<IActionResult> GetQrCode(string bookingCode)
        {
            try
            {
                var response = await _sePayService.GetQrCodeAsync(bookingCode);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating QR code.", details = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] SePayWebhookPayload payload)
        {
            Request.Headers.TryGetValue("Authorization", out var authHeader);

            try
            {
                var confirmedBooking = await _sePayService.HandleWebhookAsync(payload, authHeader);
                return Ok(new
                {
                    success = true,
                    message = "Payment verified and booking confirmed successfully.",
                    bookingCode = confirmedBooking.BookingCode,
                    totalPaid = confirmedBooking.TotalAmount
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("Ignored"))
                {
                    return Ok(new { message = ex.Message });
                }
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("already", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("đã được", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("thanh toán trước đó", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { message = ex.Message });
                }
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
