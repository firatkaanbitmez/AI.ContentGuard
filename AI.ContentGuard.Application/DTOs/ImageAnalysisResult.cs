namespace AI.ContentGuard.Application.DTOs;

public class ImageAnalysisResult
{
    public bool IsSpam { get; set; }
    public bool IsNSFW { get; set; }
    public bool IsManipulated { get; set; }
    public List<string> Issues { get; set; } = new();
}