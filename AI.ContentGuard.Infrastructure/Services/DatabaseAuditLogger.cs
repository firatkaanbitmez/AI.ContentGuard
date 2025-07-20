using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Domain.Entities;
using AI.ContentGuard.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AI.ContentGuard.Infrastructure.Services;

public class DatabaseAuditLogger : IAuditLogger
{
    private readonly ContentGuardDbContext _context;
    private readonly ILogger<DatabaseAuditLogger> _logger;

    public DatabaseAuditLogger(ContentGuardDbContext context, ILogger<DatabaseAuditLogger> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string action, object details)
    {
        try
        {
            var auditLog = new AuditLog
            {
                RequestId = ExtractRequestId(details),
                Action = action,
                Timestamp = DateTime.UtcNow,
                Details = JsonSerializer.Serialize(details)
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for action: {Action}", action);
        }
    }

    private Guid ExtractRequestId(object details)
    {
        try
        {
            var json = JsonSerializer.Serialize(details);
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("RequestId", out var requestIdElement))
            {
                return requestIdElement.GetGuid();
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return Guid.Empty;
    }
}