using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.DTOs;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;


namespace AI.ContentGuard.Application.Services;

public class RuleEngine : IRuleEngine
{
    private readonly ISpamRuleRepository _ruleRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RuleEngine> _logger;

    // Spam keywords with severity scores
    private readonly Dictionary<string, int> _spamKeywords = new()
    {
        // High severity
        { "viagra", 25 }, { "cialis", 25 }, { "casino", 20 }, { "poker", 20 },
        { "lottery", 25 }, { "prize", 15 }, { "winner", 15 }, { "congratulations", 10 },
        
        // Medium severity
        { "click here", 15 }, { "act now", 15 }, { "limited time", 10 }, { "offer expires", 10 },
        { "free money", 20 }, { "earn money", 15 }, { "work from home", 15 },
        
        // Financial scams
        { "nigerian prince", 50 }, { "bank transfer", 20 }, { "wire transfer", 20 },
        { "bitcoin", 10 }, { "cryptocurrency", 10 }, { "investment opportunity", 20 },
        
        // Phishing indicators
        { "verify account", 25 }, { "suspended account", 25 }, { "update payment", 25 },
        { "confirm identity", 20 }, { "security alert", 15 }
    };

    // Suspicious domains
    private readonly HashSet<string> _suspiciousDomains = new()
    {
        "bit.ly", "tinyurl.com", "shorturl.at", "0w.ly", "t.co",
        "mailinator.com", "tempmail.com", "guerrillamail.com"
    };

    // Pattern matchers
    private readonly List<(Regex Pattern, string Issue, int Score)> _patterns = new()
    {
        (new Regex(@"\b\d{4,}\s*USD\b", RegexOptions.IgnoreCase), "Large money amount mentioned", 15),
        (new Regex(@"[A-Z]{5,}", RegexOptions.None), "Excessive capital letters", 10),
        (new Regex(@"[!]{3,}", RegexOptions.None), "Excessive exclamation marks", 10),
        (new Regex(@"https?://\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}", RegexOptions.IgnoreCase), "IP address link", 30),
        (new Regex(@"\b(100%|guaranteed|risk.?free)\b", RegexOptions.IgnoreCase), "Unrealistic claims", 15),
        (new Regex(@"([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})", RegexOptions.IgnoreCase), "Multiple email addresses", 5)
    };

    public RuleEngine(
        ISpamRuleRepository ruleRepository,
        IMemoryCache cache,
        ILogger<RuleEngine> logger)
    {
        _ruleRepository = ruleRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<RuleEngineResult> EvaluateAsync(object content)
    {
        var result = new RuleEngineResult();

        if (content is not NormalizedContent normalizedContent)
        {
            return result;
        }

        var text = normalizedContent.PlainText?.ToLower() ?? string.Empty;

        // Get dynamic rules from database (with caching)
        var dynamicRules = await GetDynamicRulesAsync();

        // Check spam keywords
        foreach (var keyword in _spamKeywords)
        {
            if (text.Contains(keyword.Key))
            {
                result.Score += keyword.Value;
                result.Issues.Add($"Spam keyword detected: {keyword.Key}");
            }
        }

        // Check patterns
        foreach (var (pattern, issue, score) in _patterns)
        {
            var matches = pattern.Matches(normalizedContent.PlainText ?? string.Empty);
            if (matches.Count > 0)
            {
                result.Score += score * Math.Min(matches.Count, 3); // Cap multiplier at 3
                result.Issues.Add($"{issue} ({matches.Count} occurrences)");
            }
        }

        // Check suspicious domains
        var urls = ExtractUrls(normalizedContent.PlainText ?? string.Empty);
        foreach (var url in urls)
        {
            if (_suspiciousDomains.Any(domain => url.Contains(domain)))
            {
                result.Score += 20;
                result.Issues.Add($"Suspicious domain detected: {url}");
            }
        }

        // Apply dynamic rules
        foreach (var rule in dynamicRules)
        {
            var regex = new Regex(rule.Pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(text))
            {
                result.Score += rule.Score;
                result.Issues.Add($"Rule match: {rule.Pattern}");
            }
        }

        // Determine risk levels
        result.IsHighRisk = result.Score >= 70;
        result.RequiresDetailedAnalysis = result.Score >= 40 && result.Score < 70;

        _logger.LogInformation("Rule engine evaluation completed. Score: {Score}, Issues: {IssueCount}",
            result.Score, result.Issues.Count);

        return result;
    }

    private async Task<IEnumerable<SpamRule>> GetDynamicRulesAsync()
    {
        return await _cache.GetOrCreateAsync("spam_rules", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return await _ruleRepository.GetAllRulesAsync();
        });
    }

    private List<string> ExtractUrls(string text)
    {
        var urlPattern = @"https?://[^\s]+";
        var matches = Regex.Matches(text, urlPattern, RegexOptions.IgnoreCase);
        return matches.Select(m => m.Value).ToList();
    }
}