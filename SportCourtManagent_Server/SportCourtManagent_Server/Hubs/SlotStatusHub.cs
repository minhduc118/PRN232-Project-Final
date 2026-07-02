using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SportCourtManagent_Server.Hubs
{
    /// <summary>SignalR Hub for real-time slot status updates.</summary>
    public class SlotStatusHub : Hub
    {
        /// <summary>Client joins a court-specific group to receive slot updates.</summary>
        public async Task JoinCourtGroup(int courtId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"court-{courtId}");
        }

        /// <summary>Client leaves a court-specific group.</summary>
        public async Task LeaveCourtGroup(int courtId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"court-{courtId}");
        }
    }
}
