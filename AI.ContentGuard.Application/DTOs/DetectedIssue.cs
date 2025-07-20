namespace AI.ContentGuard.Application.DTOs;

public class DetectedIssue
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}