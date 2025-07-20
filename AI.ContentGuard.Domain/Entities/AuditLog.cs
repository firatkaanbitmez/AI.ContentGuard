namespace AI.ContentGuard.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public Guid RequestId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } = string.Empty;
}