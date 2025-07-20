using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Pipelines.Interfaces;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Application.Pipelines.Steps;

public class NormalizationStep : IPipelineStep
{
    private readonly ITemplateAnalysisService _templateService;
    private readonly ILogger<NormalizationStep> _logger;

    public string Name => "Content Normalization";
    public int Order => 1;

    public NormalizationStep(ITemplateAnalysisService templateService, ILogger<NormalizationStep> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<PipelineStepResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting normalization for RequestId: {RequestId}", context.RequestId);

            context.NormalizedContent = await _templateService.ParseAndNormalizeAsync(
                context.Request.ContentType,
                context.Request.Content);

            return new PipelineStepResult
            {
                Success = true,
                Metadata = { ["normalizedLength"] = context.NormalizedContent.PlainText.Length }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Normalization failed for RequestId: {RequestId}", context.RequestId);
            return new PipelineStepResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ContinuePipeline = false
            };
        }
    }

    public bool ShouldExecute(PipelineContext context) =>
        context.Request.ContentType.ToLower() != "image";
}