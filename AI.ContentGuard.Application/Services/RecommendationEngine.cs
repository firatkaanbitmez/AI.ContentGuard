using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;

namespace AI.ContentGuard.Application.Services;

public class RecommendationEngine : IRecommendationEngine
{
    public Task<string> GenerateRecommendationAsync(NormalizedContent content, ImageAnalysisResult imageResult)
    {
        // Dummy implementation
        return Task.FromResult("No recommendation available.");
    }
}