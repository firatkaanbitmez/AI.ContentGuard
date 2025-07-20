namespace AI.ContentGuard.Application.Interfaces
{
    public interface IRuleEngine
    {
        Task<RuleEngineResult> EvaluateAsync(object content);
    }

    public class RuleEngineResult
    {
        public bool IsHighRisk { get; set; }
        public bool RequiresDetailedAnalysis { get; set; }
        public int Score { get; set; }
        public List<string> Issues { get; set; } = new();
    }
}