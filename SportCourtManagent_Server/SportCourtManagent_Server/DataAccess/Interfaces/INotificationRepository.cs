using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.DataAccess.Interfaces
{
    public interface INotificationRepository
    {
        Task CreateNotificationAsync(Notification notification);
        Task CreateNotificationsBulkAsync(List<Notification> notifications);
    }
}
