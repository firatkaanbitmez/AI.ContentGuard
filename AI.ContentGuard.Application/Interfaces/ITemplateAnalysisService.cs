using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces;

public interface ITemplateAnalysisService
{
    Task<NormalizedContent> ParseAndNormalizeAsync(string contentType, string content);
}