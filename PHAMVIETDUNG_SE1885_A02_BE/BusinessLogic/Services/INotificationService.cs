using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(short accountId, string title, string message, string? articleId = null);
        Task BroadcastToAllAsync(string title, string message, short? excludeAccountId = null, string? articleId = null);
        Task<List<Notification>> GetUserNotificationsAsync(short accountId, int count = 10);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(short accountId);
    }
}
