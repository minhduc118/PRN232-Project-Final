using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DTOs.Services;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;
using System;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            try
            {
                var services = await _serviceService.GetAllServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving services.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound(new { message = $"Service with ID {id} not found." });
                }
                return Ok(service);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the service.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] ServiceRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var service = await _serviceService.CreateServiceAsync(dto);
                return CreatedAtAction(nameof(GetServiceById), new { id = service.ServiceId }, service);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the service.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _serviceService.UpdateServiceAsync(id, dto);
                if (!result)
                {
                    return NotFound(new { message = $"Service with ID {id} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the service.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                var result = await _serviceService.DeleteServiceAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Service with ID {id} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the service.", details = ex.Message });
            }
        }
    }
}
