using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SportCourtManagent_Server.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
