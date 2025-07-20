using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Domain.Entities;

namespace AI.ContentGuard.Application.Pipelines;

public class PipelineContext
{
    public Guid RequestId { get; }
    public ContentAnalysisRequestDto Request { get; }
    public NormalizedContent? NormalizedContent { get; set; }
    public SpamDetectionResult? SpamResult { get; set; }
    public ImageAnalysisResult? ImageResult { get; set; }
    public List<DetectedIssue> Issues { get; } = new();
    public int Score { get; set; }
    public string RiskLevel { get; set; } = "UNKNOWN";
    public Dictionary<string, object> Metadata { get; } = new();
    public bool HasInjection { get; set; }

    public PipelineContext(Guid requestId, ContentAnalysisRequestDto request)
    {
        RequestId = requestId;
        Request = request;
    }

    public ContentAnalysisResult ToResult()
    {
        return new ContentAnalysisResult
        {
            RequestId = RequestId,
            RiskScore = Score,
            RiskLevel = RiskLevel,
            Issues = Issues.Select(i => i.Description).ToList()
        };
    }
}