using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces;

public interface ISpamDetectionEngine
{
    Task<SpamDetectionResult> AnalyzeAsync(NormalizedContent content);
}