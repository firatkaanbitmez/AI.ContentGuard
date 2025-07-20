namespace AI.ContentGuard.Domain.Entities;

public class DetectedIssue
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
}