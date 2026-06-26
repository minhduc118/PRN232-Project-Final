using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs.Equipment;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class EquipmentInventoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipmentInventoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var equipment = await _context.EquipmentInventories
                .Include(ei => ei.Service)
                .OrderBy(ei => ei.ItemCode)
                .Select(ei => new EquipmentDto
                {
                    InventoryId = ei.InventoryId,
                    ServiceId = ei.ServiceId,
                    ServiceName = ei.Service.ServiceName,
                    ItemCode = ei.ItemCode,
                    Condition = ei.Condition.ToString(),
                    PurchaseDate = ei.PurchaseDate,
                    PurchasePrice = ei.PurchasePrice,
                    IsAvailable = ei.IsAvailable
                })
                .ToListAsync();

            return Ok(new { data = equipment });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ei = await _context.EquipmentInventories
                .Include(ei => ei.Service)
                .FirstOrDefaultAsync(x => x.InventoryId == id);

            if (ei == null)
            {
                return NotFound(new { message = "Không tìm thấy dụng cụ/thiết bị này." });
            }

            var dto = new EquipmentDto
            {
                InventoryId = ei.InventoryId,
                ServiceId = ei.ServiceId,
                ServiceName = ei.Service.ServiceName,
                ItemCode = ei.ItemCode,
                Condition = ei.Condition.ToString(),
                PurchaseDate = ei.PurchaseDate,
                PurchasePrice = ei.PurchasePrice,
                IsAvailable = ei.IsAvailable
            };

            return Ok(new { data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEquipmentRequest request)
        {
            if (await _context.EquipmentInventories.AnyAsync(e => e.ItemCode == request.ItemCode))
            {
                return BadRequest(new { message = $"Mã dụng cụ '{request.ItemCode}' đã tồn tại trong hệ thống." });
            }

            var service = await _context.Services.FindAsync(request.ServiceId);
            if (service == null)
            {
                return BadRequest(new { message = "Dịch vụ liên kết không tồn tại." });
            }

            if (!Enum.TryParse<EquipmentCondition>(request.Condition, out var conditionEnum))
            {
                return BadRequest(new { message = "Tình trạng không hợp lệ." });
            }

            var equipment = new EquipmentInventory
            {
                ServiceId = request.ServiceId,
                ItemCode = request.ItemCode,
                Condition = conditionEnum,
                PurchaseDate = request.PurchaseDate,
                PurchasePrice = request.PurchasePrice,
                IsAvailable = request.IsAvailable
            };

            await _context.EquipmentInventories.AddAsync(equipment);
            await _context.SaveChangesAsync();

            // Increment services stock qty
            service.StockQty += 1;
            _context.Services.Update(service);
            await _context.SaveChangesAsync();

            var dto = new EquipmentDto
            {
                InventoryId = equipment.InventoryId,
                ServiceId = equipment.ServiceId,
                ServiceName = service.ServiceName,
                ItemCode = equipment.ItemCode,
                Condition = equipment.Condition.ToString(),
                PurchaseDate = equipment.PurchaseDate,
                PurchasePrice = equipment.PurchasePrice,
                IsAvailable = equipment.IsAvailable
            };

            return CreatedAtAction(nameof(GetById), new { id = equipment.InventoryId }, new { message = "Thêm dụng cụ thành công.", data = dto });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipmentRequest request)
        {
            var equipment = await _context.EquipmentInventories.FindAsync(id);
            if (equipment == null)
            {
                return NotFound(new { message = "Không tìm thấy dụng cụ/thiết bị cần cập nhật." });
            }

            // Check item code uniqueness (excluding current)
            if (equipment.ItemCode != request.ItemCode && await _context.EquipmentInventories.AnyAsync(e => e.ItemCode == request.ItemCode))
            {
                return BadRequest(new { message = $"Mã dụng cụ '{request.ItemCode}' đã tồn tại." });
            }

            var service = await _context.Services.FindAsync(request.ServiceId);
            if (service == null)
            {
                return BadRequest(new { message = "Dịch vụ liên kết không tồn tại." });
            }

            if (!Enum.TryParse<EquipmentCondition>(request.Condition, out var conditionEnum))
            {
                return BadRequest(new { message = "Tình trạng không hợp lệ." });
            }

            // If service changed, update stock counts
            if (equipment.ServiceId != request.ServiceId)
            {
                var oldService = await _context.Services.FindAsync(equipment.ServiceId);
                if (oldService != null)
                {
                    oldService.StockQty = Math.Max(0, oldService.StockQty - 1);
                    _context.Services.Update(oldService);
                }
                service.StockQty += 1;
                _context.Services.Update(service);
            }

            equipment.ServiceId = request.ServiceId;
            equipment.ItemCode = request.ItemCode;
            equipment.Condition = conditionEnum;
            equipment.PurchaseDate = request.PurchaseDate;
            equipment.PurchasePrice = request.PurchasePrice;
            equipment.IsAvailable = request.IsAvailable;

            _context.EquipmentInventories.Update(equipment);
            await _context.SaveChangesAsync();

            var dto = new EquipmentDto
            {
                InventoryId = equipment.InventoryId,
                ServiceId = equipment.ServiceId,
                ServiceName = service.ServiceName,
                ItemCode = equipment.ItemCode,
                Condition = equipment.Condition.ToString(),
                PurchaseDate = equipment.PurchaseDate,
                PurchasePrice = equipment.PurchasePrice,
                IsAvailable = equipment.IsAvailable
            };

            return Ok(new { message = "Cập nhật dụng cụ thành công.", data = dto });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var equipment = await _context.EquipmentInventories.FindAsync(id);
            if (equipment == null)
            {
                return NotFound(new { message = "Không tìm thấy dụng cụ/thiết bị cần xóa." });
            }

            var service = await _context.Services.FindAsync(equipment.ServiceId);
            if (service != null)
            {
                service.StockQty = Math.Max(0, service.StockQty - 1);
                _context.Services.Update(service);
            }

            _context.EquipmentInventories.Remove(equipment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa dụng cụ thành công." });
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServicesList()
        {
            var services = await _context.Services
                .OrderBy(s => s.ServiceName)
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceName,
                    s.Category,
                    s.Price
                })
                .ToListAsync();

            return Ok(new { data = services });
        }
    }
}
