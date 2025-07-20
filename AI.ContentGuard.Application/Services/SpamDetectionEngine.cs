using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;

public class SpamDetectionEngine : ISpamDetectionEngine
{
    private readonly ILlmService _llmService;
    private readonly IRuleEngine _ruleEngine;
    private readonly ISpamRuleRepository _ruleRepository;

    public SpamDetectionEngine(
        ILlmService llmService,
        IRuleEngine ruleEngine,
        ISpamRuleRepository ruleRepository)
    {
        _llmService = llmService;
        _ruleEngine = ruleEngine;
        _ruleRepository = ruleRepository;
    }

    public async Task<SpamDetectionResult> AnalyzeAsync(NormalizedContent content)
    {
        var ruleResult = await _ruleEngine.EvaluateAsync(content);
        if (ruleResult.IsHighRisk)
            return new SpamDetectionResult { SpamScore = 100, Issues = ruleResult.Issues };

        if (ruleResult.RequiresDetailedAnalysis)
        {
            var llmResult = await _llmService.AnalyzeContent(content);
            return CombineResults(ruleResult, llmResult);
        }

        return new SpamDetectionResult
        {
            SpamScore = ruleResult.Score,
            Issues = ruleResult.Issues
        };
    }

    private SpamDetectionResult CombineResults(RuleEngineResult ruleResult, LlmResult llmResult)
    {
        return new SpamDetectionResult
        {
            IsSpam = llmResult.SpamScore > 50 || ruleResult.Score > 50,
            SpamScore = Math.Max(ruleResult.Score, llmResult.SpamScore),
            Issues = ruleResult.Issues.Concat(llmResult.Issues).ToList()
        };
    }
}