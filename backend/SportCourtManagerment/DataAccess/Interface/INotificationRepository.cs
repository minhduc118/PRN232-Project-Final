using SportCourtManagerment.Models;

namespace SportCourtManagerment.DataAccess.Interface;

public interface INotificationRepository
{
    Task CreateNotificationAsync(Notification notification);
    Task CreateNotificationsBulkAsync(List<Notification> notifications);
}
