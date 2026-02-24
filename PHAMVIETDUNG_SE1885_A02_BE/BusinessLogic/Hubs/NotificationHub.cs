using Microsoft.AspNetCore.SignalR;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Hubs;

public class NotificationHub : Hub
{
  public async Task SendNotification(string message)
  {
    await Clients.All.SendAsync("ReceiveNotification", message);
  }
}
