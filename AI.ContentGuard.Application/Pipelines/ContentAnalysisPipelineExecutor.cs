using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Pipelines.Interfaces;
using AI.ContentGuard.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace AI.ContentGuard.Application.Pipelines;

public class ContentAnalysisPipelineExecutor : IContentAnalysisPipeline
{
    private readonly IEnumerable<IPipelineStep> _steps;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<ContentAnalysisPipelineExecutor> _logger;

    public ContentAnalysisPipelineExecutor(
        IEnumerable<IPipelineStep> steps,
        IAuditLogger auditLogger,
        ILogger<ContentAnalysisPipelineExecutor> logger)
    {
        _steps = steps.OrderBy(s => s.Order);
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<ContentAnalysisResult> ProcessAsync(ContentAnalysisMessage message, CancellationToken cancellationToken)
    {
        var context = new PipelineContext(message.RequestId, new ContentAnalysisRequestDto
        {
            Content = message.Content,
            ContentType = message.ContentType
        });

        _logger.LogInformation("Starting pipeline for RequestId: {RequestId}", message.RequestId);
        await _auditLogger.LogAsync("PipelineStarted", new { message.RequestId, message.ContentType });

        try
        {
            foreach (var step in _steps)
            {
                if (!step.ShouldExecute(context))
                {
                    _logger.LogDebug("Skipping step {StepName} for RequestId: {RequestId}",
                        step.Name, message.RequestId);
                    continue;
                }

                _logger.LogDebug("Executing step {StepName} for RequestId: {RequestId}",
                    step.Name, message.RequestId);

                var result = await step.ExecuteAsync(context, cancellationToken);

                await _auditLogger.LogAsync($"Step_{step.Name}", new
                {
                    message.RequestId,
                    StepName = step.Name,
                    Success = result.Success,
                    result.Metadata
                });

                if (!result.Success)
                {
                    _logger.LogError("Step {StepName} failed for RequestId: {RequestId}: {Error}",
                        step.Name, message.RequestId, result.ErrorMessage);

                    if (!result.ContinuePipeline)
                    {
                        context.Score = 100;
                        context.RiskLevel = "ERROR";
                        context.Issues.Add(new DetectedIssue
                        {
                            Type = "SYSTEM_ERROR",
                            Description = $"Analysis failed at step: {step.Name}",
                            Severity = 10
                        });
                        break;
                    }
                }
            }

            var analysisResult = context.ToResult();

            await _auditLogger.LogAsync("PipelineCompleted", new
            {
                message.RequestId,
                analysisResult.RiskScore,
                analysisResult.RiskLevel,
                IssueCount = analysisResult.Issues.Count
            });

            _logger.LogInformation("Pipeline completed for RequestId: {RequestId}, Final Score: {Score}",
                message.RequestId, analysisResult.RiskScore);

            return analysisResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline execution failed for RequestId: {RequestId}", message.RequestId);
            await _auditLogger.LogAsync("PipelineError", new { message.RequestId, Error = ex.Message });
            throw;
        }
    }
}