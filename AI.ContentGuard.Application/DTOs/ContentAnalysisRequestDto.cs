namespace AI.ContentGuard.Application.DTOs;

public class ContentAnalysisRequestDto
{
    public string ContentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}