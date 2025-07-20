using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Pipelines.Interfaces;
using AI.ContentGuard.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Application.Pipelines.Steps;

public class ImageAnalysisStep : IPipelineStep
{
    private readonly IImageAnalysisPipeline _imageAnalysis;
    private readonly ILogger<ImageAnalysisStep> _logger;

    public string Name => "Image Analysis";
    public int Order => 4;

    public ImageAnalysisStep(IImageAnalysisPipeline imageAnalysis, ILogger<ImageAnalysisStep> logger)
    {
        _imageAnalysis = imageAnalysis;
        _logger = logger;
    }

    public async Task<PipelineStepResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        try
        {
            var imageData = Convert.FromBase64String(context.Request.Content);
            context.ImageResult = await _imageAnalysis.AnalyzeAsync(imageData);

            foreach (var issue in context.ImageResult.Issues)
            {
                context.Issues.Add(new DetectedIssue
                {
                    Type = "IMAGE_ISSUE",
                    Description = issue,
                    Severity = DetermineImageIssueSeverity(issue)
                });
            }

            _logger.LogInformation("Image analysis completed for RequestId: {RequestId}", context.RequestId);

            return new PipelineStepResult
            {
                Success = true,
                Metadata =
                {
                    ["isNSFW"] = context.ImageResult.IsNSFW,
                    ["isSpam"] = context.ImageResult.IsSpam,
                    ["isManipulated"] = context.ImageResult.IsManipulated
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image analysis failed for RequestId: {RequestId}", context.RequestId);
            return new PipelineStepResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public bool ShouldExecute(PipelineContext context) =>
        context.Request.ContentType.ToLower() == "image" && !string.IsNullOrEmpty(context.Request.Content);

    private int DetermineImageIssueSeverity(string issue)
    {
        var lowerIssue = issue.ToLower();
        return lowerIssue switch
        {
            var i when i.Contains("nsfw") || i.Contains("adult") => 9,
            var i when i.Contains("blacklisted") => 10,
            var i when i.Contains("manipulated") || i.Contains("deepfake") => 8,
            var i when i.Contains("spam") => 7,
            _ => 5
        };
    }
}