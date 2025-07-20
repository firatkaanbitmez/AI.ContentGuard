namespace AI.ContentGuard.Application.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(string action, object details);
}