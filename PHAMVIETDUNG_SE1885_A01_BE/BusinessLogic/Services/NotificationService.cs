using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public class NotificationService : INotificationService
    {
        private readonly FUNewsManagementContext _context;

        public NotificationService(FUNewsManagementContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(short accountId, string title, string message, string? articleId = null)
        {
            var notification = new Notification
            {
                AccountId = accountId,
                Title = title,
                Message = message,
                CreatedDate = DateTime.Now,
                IsRead = false,
                ArticleId = articleId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task BroadcastToAllAsync(string title, string message, short? excludeAccountId = null, string? articleId = null)
        {
            // Get ALL accounts (all roles: Admin, Staff, Lecture, Guest, etc.)
            var accounts = await _context.SystemAccounts
                .Where(a => excludeAccountId == null || a.AccountId != excludeAccountId)
                .Select(a => a.AccountId)
                .ToListAsync();

            var notifications = accounts.Select(accountId => new Notification
            {
                AccountId = accountId,
                Title = title,
                Message = message,
                CreatedDate = DateTime.Now,
                IsRead = false,
                ArticleId = articleId
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(short accountId, int count = 10)
        {
            return await _context.Set<Notification>()
                .Where(n => n.AccountId == accountId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Set<Notification>().FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(short accountId)
        {
            var notifications = await _context.Set<Notification>()
                .Where(n => n.AccountId == accountId && n.IsRead == false)
                .ToListAsync();

            if (notifications.Any())
            {
                foreach (var n in notifications)
                {
                    n.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
