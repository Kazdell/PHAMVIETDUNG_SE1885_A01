using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using System.Text.Json;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services;

public interface IAuditService
{
    Task LogActionAsync(string userEmail, string action, string entityName, string entityId, object? oldValues, object? newValues);
}

public class AuditService : IAuditService
{
    private readonly FUNewsManagementContext _context;

    public AuditService(FUNewsManagementContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(string userEmail, string action, string entityName, string entityId, object? oldValues, object? newValues)
    {
        var log = new AuditLog
        {
            UserEmail = userEmail,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
