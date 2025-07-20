using AI.ContentGuard.Domain.Entities;

namespace AI.ContentGuard.Domain.Entities;

public class AnalysisResult
{
    public Guid RequestId { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<DetectedIssue> Issues { get; set; } = new();
    public DateTime CompletedAt { get; set; }
    public Dictionary<string, object> AnalysisMetadata { get; set; } = new();
}