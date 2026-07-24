using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Hubs;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationRepository(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            try
            {
                // Push real-time notification via SignalR
                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        notification.NotificationId,
                        notification.UserId,
                        notification.Title,
                        Type = notification.Type.ToString(),
                        notification.IsRead,
                        CreatedAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR Notification Error] {ex.Message}");
            }
        }

        public async Task CreateNotificationsBulkAsync(List<Notification> notifications)
        {
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                try
                {
                    // Push each in real-time
                    await _hubContext.Clients.User(notification.UserId.ToString())
                        .SendAsync("ReceiveNotification", new
                        {
                            notification.NotificationId,
                            notification.UserId,
                            notification.Title,
                            Type = notification.Type.ToString(),
                            notification.IsRead,
                            CreatedAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR Bulk Notification Error] {ex.Message}");
                }
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int limit = 50)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notif == null) return false;

            notif.IsRead = true;
            _context.Notifications.Update(notif);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!unread.Any()) return true;

            foreach (var notif in unread)
            {
                notif.IsRead = true;
            }

            _context.Notifications.UpdateRange(unread);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
