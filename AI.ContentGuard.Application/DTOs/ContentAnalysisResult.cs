namespace AI.ContentGuard.Application.DTOs;

public class ContentAnalysisResult
{
    public Guid RequestId { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> Issues { get; set; } = new();
}