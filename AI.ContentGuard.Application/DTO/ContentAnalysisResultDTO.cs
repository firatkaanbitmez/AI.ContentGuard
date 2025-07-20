public class ContentAnalysisResultDTO
{
    public int Score { get; set; }
    public string RiskLevel { get; set; }
    public List<IssueDTO> Issues { get; set; }
}