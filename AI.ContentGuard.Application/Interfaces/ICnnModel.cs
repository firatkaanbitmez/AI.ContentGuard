namespace AI.ContentGuard.Application.Interfaces;

public class CnnResult
{
    public double NsfwProbability { get; set; }
    public bool RequiresDetailedAnalysis { get; set; }
    public int RiskScore { get; set; }
    public List<string> DetectedIssues { get; set; } = new();
}

public interface ICnnModel
{
    Task<CnnResult> Analyze(byte[] imageData);
}