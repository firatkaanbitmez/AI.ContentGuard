namespace AI.ContentGuard.Application.DTOs;

public class SpamDetectionResult
{
    public bool IsSpam { get; set; }
    public int SpamScore { get; set; }
    public List<string> Issues { get; set; } = new();
}