using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Application.Services;

public class ScoreCalculator : IScoreCalculator
{
    private readonly ILogger<ScoreCalculator> _logger;

    // Risk Score Weights
    private const int SQL_INJECTION_SCORE = 30;
    private const int BANNED_LINK_SCORE = 20;
    private const int LLM_SPAM_SCORE = 40;
    private const int IMAGE_SPAM_NSFW_SCORE = 50;
    private const int BLACKLIST_KEYWORD_SCORE = 15;
    private const int PHISHING_SCORE = 45;
    private const int MALWARE_SCORE = 60;

    public ScoreCalculator(ILogger<ScoreCalculator> logger)
    {
        _logger = logger;
    }

    public int CalculateRiskScore(SpamDetectionResult spamResult, bool hasInjection, ImageAnalysisResult imageResult)
    {
        var totalScore = 0;
        var scoreBreakdown = new List<string>();

        try
        {
            // Base spam score
            totalScore += spamResult.SpamScore;
            if (spamResult.SpamScore > 0)
            {
                scoreBreakdown.Add($"Spam Detection: {spamResult.SpamScore}");
            }

            // Injection detection penalty
            if (hasInjection)
            {
                totalScore += SQL_INJECTION_SCORE;
                scoreBreakdown.Add($"Injection Detection: {SQL_INJECTION_SCORE}");
            }

            // Process spam-specific issues
            foreach (var issue in spamResult.Issues)
            {
                var issueScore = CalculateIssueScore(issue);
                totalScore += issueScore;
                if (issueScore > 0)
                {
                    scoreBreakdown.Add($"{issue}: {issueScore}");
                }
            }

            // Image analysis scores
            if (imageResult.Issues.Any())
            {
                foreach (var imageIssue in imageResult.Issues)
                {
                    var imageScore = CalculateImageIssueScore(imageIssue);
                    totalScore += imageScore;
                    if (imageScore > 0)
                    {
                        scoreBreakdown.Add($"Image {imageIssue}: {imageScore}");
                    }
                }
            }

            // Additional image flags
            if (imageResult.IsNSFW)
            {
                totalScore += IMAGE_SPAM_NSFW_SCORE;
                scoreBreakdown.Add($"NSFW Content: {IMAGE_SPAM_NSFW_SCORE}");
            }

            if (imageResult.IsSpam)
            {
                totalScore += IMAGE_SPAM_NSFW_SCORE;
                scoreBreakdown.Add($"Image Spam: {IMAGE_SPAM_NSFW_SCORE}");
            }

            if (imageResult.IsManipulated)
            {
                totalScore += 25;
                scoreBreakdown.Add($"Image Manipulation: 25");
            }

            // Cap the maximum score
            totalScore = Math.Min(totalScore, 100);

            _logger.LogDebug("Risk score calculation completed. Total: {TotalScore}, Breakdown: {Breakdown}",
                totalScore, string.Join(", ", scoreBreakdown));

            return totalScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating risk score");
            return 100; // Return maximum risk on error
        }
    }

    public string GetRiskLevel(int score)
    {
        return score switch
        {
            >= 81 => "HIGH_RISK",
            >= 61 => "MEDIUM_RISK",
            >= 41 => "LOW_RISK",
            _ => "SAFE"
        };
    }

    private int CalculateIssueScore(string issue)
    {
        var lowerIssue = issue.ToLower();

        return lowerIssue switch
        {
            var i when i.Contains("sql injection") || i.Contains("script injection") => SQL_INJECTION_SCORE,
            var i when i.Contains("phishing") || i.Contains("fraud") => PHISHING_SCORE,
            var i when i.Contains("malware") || i.Contains("virus") => MALWARE_SCORE,
            var i when i.Contains("banned") && i.Contains("link") => BANNED_LINK_SCORE,
            var i when i.Contains("blacklist") => BLACKLIST_KEYWORD_SCORE,
            var i when i.Contains("spam") => 25,
            var i when i.Contains("suspicious") => 15,
            var i when i.Contains("policy violation") => 20,
            _ => 10 // Default penalty for any detected issue
        };
    }

    private int CalculateImageIssueScore(string imageIssue)
    {
        var lowerIssue = imageIssue.ToLower();

        return lowerIssue switch
        {
            var i when i.Contains("nsfw") || i.Contains("adult") => IMAGE_SPAM_NSFW_SCORE,
            var i when i.Contains("blacklisted") => 60,
            var i when i.Contains("spam") => 35,
            var i when i.Contains("manipulation") || i.Contains("deepfake") => 30,
            var i when i.Contains("invalid") || i.Contains("corrupt") => 20,
            var i when i.Contains("suspicious") => 15,
            _ => 10
        };
    }

    public ScoreBreakdown GetDetailedScoreBreakdown(SpamDetectionResult spamResult, bool hasInjection, ImageAnalysisResult imageResult)
    {
        var breakdown = new ScoreBreakdown();

        breakdown.BaseSpamScore = spamResult.SpamScore;
        breakdown.InjectionPenalty = hasInjection ? SQL_INJECTION_SCORE : 0;

        breakdown.IssueScores = spamResult.Issues.ToDictionary(
            issue => issue,
            issue => CalculateIssueScore(issue)
        );

        breakdown.ImageScores = imageResult.Issues.ToDictionary(
            issue => issue,
            issue => CalculateImageIssueScore(issue)
        );

        if (imageResult.IsNSFW)
            breakdown.ImageScores["NSFW_Flag"] = IMAGE_SPAM_NSFW_SCORE;

        if (imageResult.IsSpam)
            breakdown.ImageScores["Spam_Flag"] = IMAGE_SPAM_NSFW_SCORE;

        if (imageResult.IsManipulated)
            breakdown.ImageScores["Manipulation_Flag"] = 25;

        breakdown.TotalScore = Math.Min(
            breakdown.BaseSpamScore +
            breakdown.InjectionPenalty +
            breakdown.IssueScores.Values.Sum() +
            breakdown.ImageScores.Values.Sum(),
            100);

        return breakdown;
    }
}

public class ScoreBreakdown
{
    public int BaseSpamScore { get; set; }
    public int InjectionPenalty { get; set; }
    public Dictionary<string, int> IssueScores { get; set; } = new();
    public Dictionary<string, int> ImageScores { get; set; } = new();
    public int TotalScore { get; set; }
}