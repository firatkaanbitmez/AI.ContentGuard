namespace AI.ContentGuard.Application.DTOs;

public class ContentAnalysisMessage
{
    public Guid RequestId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}