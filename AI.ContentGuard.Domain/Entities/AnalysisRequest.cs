namespace AI.ContentGuard.Domain.Entities;

public class AnalysisRequest
{
    public Guid Id { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CustomerProfile { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}