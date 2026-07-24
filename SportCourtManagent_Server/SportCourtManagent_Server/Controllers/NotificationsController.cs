using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        private int? GetUserId()
        {
            var claimVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimVal, out int id))
            {
                return id;
            }
            return null;
        }

        // GET /api/notifications
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int limit = 50)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized("Không tìm thấy thông tin đăng nhập.");

            var items = await _notificationRepository.GetUserNotificationsAsync(userId.Value, limit);
            return Ok(items);
        }

        // GET /api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized("Không tìm thấy thông tin đăng nhập.");

            var count = await _notificationRepository.GetUnreadCountAsync(userId.Value);
            return Ok(new { Count = count });
        }

        // PUT /api/notifications/{id}/read
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized("Không tìm thấy thông tin đăng nhập.");

            var success = await _notificationRepository.MarkAsReadAsync(id, userId.Value);
            if (!success) return NotFound("Không tìm thấy thông báo hoặc thông báo không thuộc về bạn.");

            return Ok(new { Message = "Đã đánh dấu thông báo là đã đọc." });
        }

        // PUT /api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized("Không tìm thấy thông tin đăng nhập.");

            await _notificationRepository.MarkAllAsReadAsync(userId.Value);
            return Ok(new { Message = "Đã đánh dấu tất cả thông báo là đã đọc." });
        }
    }
}
