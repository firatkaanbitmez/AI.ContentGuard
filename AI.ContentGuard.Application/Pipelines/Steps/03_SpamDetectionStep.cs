// AI.ContentGuard.Application/Pipelines/Steps/03_SpamDetectionStep.cs
using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Pipelines.Interfaces;
using Microsoft.Extensions.Logging;
using DomainDetectedIssue = AI.ContentGuard.Domain.Entities.DetectedIssue; // Use alias

namespace AI.ContentGuard.Application.Pipelines.Steps;

public class SpamDetectionStep : IPipelineStep
{
    private readonly ISpamDetectionEngine _spamEngine;
    private readonly ILogger<SpamDetectionStep> _logger;

    public string Name => "Spam Detection";
    public int Order => 3;

    public SpamDetectionStep(ISpamDetectionEngine spamEngine, ILogger<SpamDetectionStep> logger)
    {
        _spamEngine = spamEngine;
        _logger = logger;
    }

    public async Task<PipelineStepResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        try
        {
            if (context.NormalizedContent == null)
                return new PipelineStepResult { Success = true };

            context.SpamResult = await _spamEngine.AnalyzeAsync(context.NormalizedContent);

            foreach (var issue in context.SpamResult.Issues)
            {
                context.Issues.Add(new DomainDetectedIssue
                {
                    Type = "SPAM",
                    Description = issue,
                    Severity = context.SpamResult.SpamScore > 70 ? 8 : 5
                });
            }

            _logger.LogInformation("Spam detection completed for RequestId: {RequestId}, Score: {Score}",
                context.RequestId, context.SpamResult.SpamScore);

            return new PipelineStepResult
            {
                Success = true,
                Metadata = { ["spamScore"] = context.SpamResult.SpamScore }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spam detection failed for RequestId: {RequestId}", context.RequestId);
            return new PipelineStepResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public bool ShouldExecute(PipelineContext context) =>
        context.NormalizedContent != null;
}