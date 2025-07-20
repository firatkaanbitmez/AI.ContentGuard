public class PipelineContext
{
    public ContentAnalysisRequestDTO Request { get; }
    public List<IssueDTO> Issues { get; } = new();
    public int Score { get; set; }
    public string RiskLevel { get; set; }

    public PipelineContext(ContentAnalysisRequestDTO request)
    {
        Request = request;
    }

    public ContentAnalysisResultDTO ToResultDTO()
    {
        return new ContentAnalysisResultDTO
        {
            Score = Score,
            RiskLevel = RiskLevel,
            Issues = Issues
        };
    }
}