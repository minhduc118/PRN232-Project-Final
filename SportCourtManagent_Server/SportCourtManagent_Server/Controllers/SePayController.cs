using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Payments;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SePayController : ControllerBase
    {
        private readonly ISePayService _sePayService;
        private readonly IInMemoryBookingRepository _inMemoryBookingRepository;
        private readonly IBookingRepository _bookingRepository;

        public SePayController(
            ISePayService sePayService,
            IInMemoryBookingRepository inMemoryBookingRepository,
            IBookingRepository bookingRepository)
        {
            _sePayService = sePayService;
            _inMemoryBookingRepository = inMemoryBookingRepository;
            _bookingRepository = bookingRepository;
        }

        [HttpGet("status/{bookingCode}")]
        public async Task<IActionResult> GetStatus(string bookingCode)
        {
            try
            {
                // 1. Try in-memory first (typically pending)
                var memBooking = await _inMemoryBookingRepository.GetByCodeAsync(bookingCode);
                if (memBooking != null)
                {
                    return Ok(new { bookingCode = bookingCode, status = memBooking.Status });
                }

                // 2. Try database
                var dbBooking = (await _bookingRepository.GetAllAsync())
                    .FirstOrDefault(b => string.Equals(b.BookingCode, bookingCode, StringComparison.OrdinalIgnoreCase));
                if (dbBooking != null)
                {
                    return Ok(new { bookingCode = bookingCode, status = dbBooking.Status.ToString() });
                }

                return NotFound(new { message = "Booking session has expired or does not exist." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error checking status.", details = ex.Message });
            }
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
