using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DTOs.Bookings;
using SportCourtManagent_Server.Services.Interfaces;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ICourtBookingService _courtBookingService;

        public BookingController(ICourtBookingService courtBookingService)
        {
            _courtBookingService = courtBookingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto dto)
        {
            int userId = 1; 

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _courtBookingService.CreateBookingAsync(dto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new 
                { 
                    message = "An unexpected error occurred while processing your booking.", 
                    details = ex.Message 
                });
            }
        }
        
    }
}
