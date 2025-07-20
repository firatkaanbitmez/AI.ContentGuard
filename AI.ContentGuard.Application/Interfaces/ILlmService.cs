using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces;

public class LlmResult
{
    public int SpamScore { get; set; }
    public List<string> Issues { get; set; } = new();
}

public interface ILlmService
{
    Task<LlmResult> AnalyzeImage(byte[] imageData);
    Task<LlmResult> AnalyzeContent(NormalizedContent content);
}