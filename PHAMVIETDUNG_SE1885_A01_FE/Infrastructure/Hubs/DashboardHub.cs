using Microsoft.AspNetCore.SignalR;

namespace PHAMVIETDUNG_SE1885_A01_FE.Infrastructure.Hubs
{
    public class DashboardHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"SignalR Client Connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }
    }
}
