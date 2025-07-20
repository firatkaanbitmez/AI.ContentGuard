using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AI.ContentGuard.Application.Services;

public class InjectionValidator : IInjectionValidator
{
    private readonly Microsoft.Extensions.Logging.ILogger<InjectionValidator> _logger;

    // SQL Injection patterns
    private readonly List<Regex> _sqlPatterns = new()
    {
        new Regex(@"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|CREATE|ALTER|EXEC|EXECUTE)\b)", RegexOptions.IgnoreCase),
        new Regex(@"(--|#|\/\*|\*\/|@@|@)", RegexOptions.IgnoreCase),
        new Regex(@"(\bOR\b\s*\d+\s*=\s*\d+|\bAND\b\s*\d+\s*=\s*\d+)", RegexOptions.IgnoreCase),
        new Regex(@"('|""|´|`)", RegexOptions.IgnoreCase),
        new Regex(@"(\bEXEC\b\s*\(|\bEXECUTE\b\s*\()", RegexOptions.IgnoreCase),
        new Regex(@"(xp_|sp_|OPENROWSET|OPENDATASOURCE|OPENQUERY)", RegexOptions.IgnoreCase),
        new Regex(@"(WAITFOR\s+DELAY|BENCHMARK\s*\(|SLEEP\s*\()", RegexOptions.IgnoreCase)
    };

    // XSS patterns
    private readonly List<Regex> _xssPatterns = new()
    {
        new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
        new Regex(@"javascript\s*:", RegexOptions.IgnoreCase),
        new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase),
        new Regex(@"<iframe[^>]*>", RegexOptions.IgnoreCase),
        new Regex(@"<object[^>]*>", RegexOptions.IgnoreCase),
        new Regex(@"<embed[^>]*>", RegexOptions.IgnoreCase),
        new Regex(@"<link[^>]*href[^>]*>", RegexOptions.IgnoreCase),
        new Regex(@"eval\s*\(", RegexOptions.IgnoreCase),
        new Regex(@"expression\s*\(", RegexOptions.IgnoreCase),
        new Regex(@"vbscript\s*:", RegexOptions.IgnoreCase),
        new Regex(@"<img[^>]+src\s*=\s*[""]javascript:", RegexOptions.IgnoreCase)
    };

    // NoSQL Injection patterns
    private readonly List<Regex> _noSqlPatterns = new()
    {
        new Regex(@"\$\w+\s*:", RegexOptions.IgnoreCase),
        new Regex(@"{\s*\$\w+\s*:", RegexOptions.IgnoreCase),
        new Regex(@"\$where\s*:", RegexOptions.IgnoreCase),
        new Regex(@"\.aggregate\s*\(", RegexOptions.IgnoreCase)
    };

    public InjectionValidator(Microsoft.Extensions.Logging.ILogger<InjectionValidator> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HasInjectionAsync(NormalizedContent content)
    {
        var detectedInjections = new List<string>();

        // Check plain text
        if (!string.IsNullOrEmpty(content.PlainText))
        {
            CheckForInjections(content.PlainText, detectedInjections);
        }

        // Check HTML if present
        if (!string.IsNullOrEmpty(content.Html))
        {
            CheckForXss(content.Html, detectedInjections);
        }

        // Check JSON if present
        if (!string.IsNullOrEmpty(content.Json))
        {
            CheckForNoSqlInjection(content.Json, detectedInjections);
        }

        if (detectedInjections.Any())
        {
            _logger.LogWarning("Injection attempts detected: {Injections}",
                string.Join(", ", detectedInjections));
            return true;
        }

        return await Task.FromResult(false);
    }

    private void CheckForInjections(string text, List<string> detectedInjections)
    {
        foreach (var pattern in _sqlPatterns)
        {
            if (pattern.IsMatch(text))
            {
                detectedInjections.Add($"SQL Injection pattern: {pattern}");
            }
        }
    }

    private void CheckForXss(string html, List<string> detectedInjections)
    {
        foreach (var pattern in _xssPatterns)
        {
            if (pattern.IsMatch(html))
            {
                detectedInjections.Add($"XSS pattern: {pattern}");
            }
        }
    }

    private void CheckForNoSqlInjection(string json, List<string> detectedInjections)
    {
        foreach (var pattern in _noSqlPatterns)
        {
            if (pattern.IsMatch(json))
            {
                detectedInjections.Add($"NoSQL Injection pattern: {pattern}");
            }
        }
    }
}