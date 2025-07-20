using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces;

public interface IContentAnalysisPipeline
{
    Task<ContentAnalysisResult> ProcessAsync(ContentAnalysisMessage message, CancellationToken cancellationToken);
}