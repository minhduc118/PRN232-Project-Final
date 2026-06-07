using SportCourtManagerment.Data;
using SportCourtManagerment.DataAccess.Interface;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.DataAccess.Implementation;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _db;

    public NotificationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }

    public async Task CreateNotificationsBulkAsync(List<Notification> notifications)
    {
        _db.Notifications.AddRange(notifications);
        await _db.SaveChangesAsync();
    }
}
