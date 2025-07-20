using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces;

public interface IRecommendationEngine
{
    Task<string> GenerateRecommendationAsync(NormalizedContent content, ImageAnalysisResult imageResult);
}