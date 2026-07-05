using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Implementation
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateNotificationsBulkAsync(List<Notification> notifications)
        {
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }
    }
}
