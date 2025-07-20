using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Application.Services;

public class ContentAnalysisPipeline : IContentAnalysisPipeline
{
    private readonly ITemplateAnalysisService _templateAnalysis;
    private readonly ISpamDetectionEngine _spamDetection;
    private readonly IImageAnalysisPipeline _imageAnalysis;
    private readonly IInjectionValidator _injectionValidator;
    private readonly IScoreCalculator _scoreCalculator;
    private readonly IRecommendationEngine _recommendationEngine;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<ContentAnalysisPipeline> _logger;

    public ContentAnalysisPipeline(
        ITemplateAnalysisService templateAnalysis,
        ISpamDetectionEngine spamDetection,
        IImageAnalysisPipeline imageAnalysis,
        IInjectionValidator injectionValidator,
        IScoreCalculator scoreCalculator,
        IRecommendationEngine recommendationEngine,
        IAuditLogger auditLogger,
        ILogger<ContentAnalysisPipeline> logger)
    {
        _templateAnalysis = templateAnalysis;
        _spamDetection = spamDetection;
        _imageAnalysis = imageAnalysis;
        _injectionValidator = injectionValidator;
        _scoreCalculator = scoreCalculator;
        _recommendationEngine = recommendationEngine;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<ContentAnalysisResult> ProcessAsync(ContentAnalysisMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting content analysis for RequestId: {RequestId}", message.RequestId);

        try
        {
            await _auditLogger.LogAsync("AnalysisStarted", new { RequestId = message.RequestId, ContentType = message.ContentType });

            var result = new ContentAnalysisResult
            {
                RequestId = message.RequestId,
                Issues = new List<string>()
            };

            // Step 1: Template Analysis & Normalization
            var normalized = await _templateAnalysis.ParseAndNormalizeAsync(message.ContentType, message.Content);

            // Step 2: Spam Detection
            var spamResult = await _spamDetection.AnalyzeAsync(normalized);
            result.Issues.AddRange(spamResult.Issues);

            // Step 3: Injection Validation
            var hasInjection = await _injectionValidator.HasInjectionAsync(normalized);
            if (hasInjection)
            {
                result.Issues.Add("Potential injection attack detected");
            }

            // Step 4: Image Analysis (if applicable)
            var imageResult = new ImageAnalysisResult();
            if (message.ContentType.ToLower() == "image" && !string.IsNullOrEmpty(message.Content))
            {
                try
                {
                    var imageData = Convert.FromBase64String(message.Content);
                    imageResult = await _imageAnalysis.AnalyzeAsync(imageData);
                    result.Issues.AddRange(imageResult.Issues);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image data for RequestId: {RequestId}", message.RequestId);
                    result.Issues.Add("Image processing error");
                }
            }

            // Step 5: Calculate Risk Score
            result.RiskScore = _scoreCalculator.CalculateRiskScore(spamResult, hasInjection, imageResult);
            result.RiskLevel = _scoreCalculator.GetRiskLevel(result.RiskScore);

            // Step 6: Generate Recommendations (if needed)
            if (result.RiskScore > 40)
            {
                var recommendation = await _recommendationEngine.GenerateRecommendationAsync(normalized, imageResult);
                // Store recommendation for later use
            }

            await _auditLogger.LogAsync("AnalysisCompleted", new
            {
                RequestId = message.RequestId,
                RiskScore = result.RiskScore,
                RiskLevel = result.RiskLevel,
                IssuesCount = result.Issues.Count
            });

            _logger.LogInformation("Content analysis completed for RequestId: {RequestId}, RiskScore: {RiskScore}",
                message.RequestId, result.RiskScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during content analysis for RequestId: {RequestId}", message.RequestId);
            await _auditLogger.LogAsync("AnalysisError", new { RequestId = message.RequestId, Error = ex.Message });

            return new ContentAnalysisResult
            {
                RequestId = message.RequestId,
                RiskScore = 100,
                RiskLevel = "Error",
                Issues = new List<string> { "Analysis failed due to system error" }
            };
        }
    }
}