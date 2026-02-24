using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;
using System.Text.Json;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services;

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
        var detailsObj = new
        {
            Old = oldValues,
            New = newValues
        };

        var log = new AuditLog
        {
            UserEmail = userEmail,
            Action = action,
            TableName = entityName,
            RecordID = entityId,
            Timestamp = DateTime.UtcNow,
            Details = JsonSerializer.Serialize(detailsObj)
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
