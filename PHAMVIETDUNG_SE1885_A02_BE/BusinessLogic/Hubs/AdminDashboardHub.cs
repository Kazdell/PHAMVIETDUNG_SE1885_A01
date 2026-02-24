using Microsoft.AspNetCore.SignalR;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Hubs
{
    public class AdminDashboardHub : Hub
    {
        // Clients connect to listen.
        // We can add server-client methods here if needed, e.g. "SubscribeToStats"
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
