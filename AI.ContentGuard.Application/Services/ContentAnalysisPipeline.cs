using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Domain.Entities;

namespace AI.ContentGuard.Application.Services;

public class ContentAnalysisPipeline : IContentAnalysisPipeline
{
    private readonly ITemplateAnalysisService _templateAnalysis;
    private readonly ISpamDetectionEngine _spamDetection;
    private readonly IImageAnalysisPipeline _imageAnalysis;
    private readonly IInjectionValidator _injectionValidator;
    private readonly IScoreCalculator _scoreCalculator;
    private readonly IRecommendationEngine _recommendationEngine;

    public ContentAnalysisPipeline(
        ITemplateAnalysisService templateAnalysis,
        ISpamDetectionEngine spamDetection,
        IImageAnalysisPipeline imageAnalysis,
        IInjectionValidator injectionValidator,
        IScoreCalculator scoreCalculator,
        IRecommendationEngine recommendationEngine)
    {
        _templateAnalysis = templateAnalysis;
        _spamDetection = spamDetection;
        _imageAnalysis = imageAnalysis;
        _injectionValidator = injectionValidator;
        _scoreCalculator = scoreCalculator;
        _recommendationEngine = recommendationEngine;
    }

    public async Task<ContentAnalysisResult> ProcessAsync(ContentAnalysisMessage message, CancellationToken cancellationToken)
    {
        var normalized = await _templateAnalysis.ParseAndNormalizeAsync(message.ContentType, message.Content);
        var spamResult = await _spamDetection.AnalyzeAsync(normalized);
        var hasInjection = await _injectionValidator.HasInjectionAsync(normalized);
        var imageResult = message.ContentType == "image"
            ? await _imageAnalysis.AnalyzeAsync(Convert.FromBase64String(message.Content))
            : new ImageAnalysisResult();

        var score = _scoreCalculator.CalculateRiskScore(spamResult, hasInjection, imageResult);
        var riskLevel = _scoreCalculator.GetRiskLevel(score);

        var result = new ContentAnalysisResult
        {
            RequestId = message.RequestId,
            RiskScore = score,
            RiskLevel = riskLevel,
            Issues = spamResult.Issues
        };

        return result;
    }
}