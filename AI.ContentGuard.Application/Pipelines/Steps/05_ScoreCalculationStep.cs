using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Pipelines.Interfaces;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Application.Pipelines.Steps;

public class ScoreCalculationStep : IPipelineStep
{
    private readonly IScoreCalculator _scoreCalculator;
    private readonly ILogger<ScoreCalculationStep> _logger;

    public string Name => "Risk Score Calculation";
    public int Order => 5;

    public ScoreCalculationStep(IScoreCalculator scoreCalculator, ILogger<ScoreCalculationStep> logger)
    {
        _scoreCalculator = scoreCalculator;
        _logger = logger;
    }

    public async Task<PipelineStepResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        try
        {
            context.Score = _scoreCalculator.CalculateRiskScore(
                context.SpamResult ?? new SpamDetectionResult(),
                context.HasInjection,
                context.ImageResult ?? new ImageAnalysisResult());

            context.RiskLevel = _scoreCalculator.GetRiskLevel(context.Score);

            _logger.LogInformation("Risk score calculated for RequestId: {RequestId}, Score: {Score}, Level: {Level}",
                context.RequestId, context.Score, context.RiskLevel);

            return await Task.FromResult(new PipelineStepResult
            {
                Success = true,
                Metadata =
                {
                    ["finalScore"] = context.Score,
                    ["riskLevel"] = context.RiskLevel
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Score calculation failed for RequestId: {RequestId}", context.RequestId);
            return new PipelineStepResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public bool ShouldExecute(PipelineContext context) => true;
}
