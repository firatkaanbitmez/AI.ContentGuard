using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Shared.Exceptions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Application.Services;

public class TemplateAnalysisService : ITemplateAnalysisService
{
    private readonly ILogger<TemplateAnalysisService> _logger;

    public TemplateAnalysisService(ILogger<TemplateAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<NormalizedContent> ParseAndNormalizeAsync(string contentType, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new NormalizedContent();
        }

        try
        {
            return contentType.ToLower() switch
            {
                "html" => await ParseHtmlAsync(content),
                "json" => await ParseJsonAsync(content),
                "plain" or "text" => await ParsePlainTextAsync(content),
                "image" => new NormalizedContent { PlainText = "[IMAGE_CONTENT]" },
                _ => throw new ValidationException($"Unsupported content type: {contentType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing content of type: {ContentType}", contentType);
            throw new ContentGuardException($"Failed to parse {contentType} content: {ex.Message}");
        }
    }

    private async Task<NormalizedContent> ParseHtmlAsync(string htmlContent)
    {
        var normalized = new NormalizedContent
        {
            Html = htmlContent
        };

        // Strip HTML tags to get plain text
        var plainText = StripHtmlTags(htmlContent);
        normalized.PlainText = CleanWhitespace(plainText);

        // Extract structured data if possible
        try
        {
            var structuredData = ExtractStructuredData(htmlContent);
            if (structuredData.Any())
            {
                normalized.Json = JsonSerializer.Serialize(structuredData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract structured data from HTML");
        }

        return await Task.FromResult(normalized);
    }

    private async Task<NormalizedContent> ParseJsonAsync(string jsonContent)
    {
        var normalized = new NormalizedContent
        {
            Json = jsonContent
        };

        try
        {
            // Validate and minify JSON
            var jsonDocument = JsonDocument.Parse(jsonContent);
            normalized.Json = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            // Extract text values for plain text analysis
            var textValues = ExtractTextFromJson(jsonDocument.RootElement);
            normalized.PlainText = string.Join(" ", textValues);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON content");
            throw new ValidationException("Invalid JSON format");
        }

        return await Task.FromResult(normalized);
    }

    private async Task<NormalizedContent> ParsePlainTextAsync(string textContent)
    {
        var normalized = new NormalizedContent
        {
            PlainText = CleanWhitespace(textContent)
        };

        return await Task.FromResult(normalized);
    }

    private string StripHtmlTags(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Remove script and style elements completely
        html = Regex.Replace(html, @"<(script|style)[^>]*>.*?</\1>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Remove HTML comments
        html = Regex.Replace(html, @"<!--.*?-->", "", RegexOptions.Singleline);

        // Remove all HTML tags
        html = Regex.Replace(html, @"<[^>]+>", " ");

        // Decode HTML entities
        html = System.Net.WebUtility.HtmlDecode(html);

        return html;
    }

    private string CleanWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Replace multiple whitespace with single space
        text = Regex.Replace(text, @"\s+", " ");

        return text.Trim();
    }

    private Dictionary<string, object> ExtractStructuredData(string html)
    {
        var structuredData = new Dictionary<string, object>();

        // Extract meta tags
        var metaMatches = Regex.Matches(html, @"<meta[^>]+name=[""']([^""']+)[""'][^>]+content=[""']([^""']+)[""'][^>]*>", RegexOptions.IgnoreCase);
        var metaData = new Dictionary<string, string>();

        foreach (Match match in metaMatches)
        {
            metaData[match.Groups[1].Value] = match.Groups[2].Value;
        }

        if (metaData.Any())
        {
            structuredData["meta"] = metaData;
        }

        // Extract links
        var linkMatches = Regex.Matches(html, @"<a[^>]+href=[""']([^""']+)[""'][^>]*>([^<]*)</a>", RegexOptions.IgnoreCase);
        var links = new List<object>();

        foreach (Match match in linkMatches)
        {
            links.Add(new { href = match.Groups[1].Value, text = match.Groups[2].Value });
        }

        if (links.Any())
        {
            structuredData["links"] = links;
        }

        return structuredData;
    }

    private List<string> ExtractTextFromJson(JsonElement element)
    {
        var textValues = new List<string>();

        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var stringValue = element.GetString();
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    textValues.Add(stringValue);
                }
                break;

            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    textValues.AddRange(ExtractTextFromJson(property.Value));
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    textValues.AddRange(ExtractTextFromJson(item));
                }
                break;
        }

        return textValues;
    }
}