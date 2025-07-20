using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AI.ContentGuard.Infrastructure.AI.ImageAnalysis;

public class LlmImageAnalyzer : ILlmService
{
    private readonly ILogger<LlmImageAnalyzer> _logger;
    private readonly OpenAIClient _openAIClient;
    private readonly string _deploymentName;

    public LlmImageAnalyzer(ILogger<LlmImageAnalyzer> logger, IConfiguration configuration)
    {
        _logger = logger;

        var endpoint = configuration["AI:AzureOpenAI:Endpoint"];
        var apiKey = configuration["AI:AzureOpenAI:ApiKey"];
        _deploymentName = configuration["AI:AzureOpenAI:DeploymentName"] ?? "gpt-4-vision";

        _openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
    }

    public async Task<LlmResult> AnalyzeImage(byte[] imageData)
    {
        try
        {
            var base64Image = Convert.ToBase64String(imageData);

            var messages = new[]
            {
                new ChatRequestMessage(ChatRole.System,
                    "You are an image content moderator. Analyze images for spam, NSFW content, manipulation, and policy violations."),
                new ChatRequestMessage(ChatRole.User,
                    $"Analyze this image for: 1) Spam indicators 2) NSFW content 3) Manipulation/deepfakes 4) Policy violations. " +
                    $"Return a JSON with spam_score (0-100), issues array, and detailed analysis.")
            };

            var options = new ChatCompletionsOptions
            {
                DeploymentName = _deploymentName,
                Messages = { messages[0], messages[1] },
                MaxTokens = 500,
                Temperature = 0.3f
            };

            var response = await _openAIClient.GetChatCompletionsAsync(options);

            return ParseLlmResponse(response.Value.Choices[0].Message.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM image analysis failed");
            return new LlmResult { SpamScore = 0, Issues = new List<string> { "LLM analysis unavailable" } };
        }
    }

    public async Task<LlmResult> AnalyzeContent(NormalizedContent content)
    {
        try
        {
            var prompt = $@"Analyze this content for spam, phishing, and harmful content:
Content: {content.PlainText}

Evaluate for:
1. Spam indicators (score 0-100)
2. Phishing attempts
3. Scam patterns
4. Harmful content

Return JSON with spam_score and issues array.";

            var messages = new[]
            {
                new ChatRequestMessage(ChatRole.System,
                    "You are a content security expert. Analyze text for spam, phishing, and harmful content."),
                new ChatRequestMessage(ChatRole.User, prompt)
            };

            var options = new ChatCompletionsOptions
            {
                DeploymentName = _deploymentName,
                Messages = { messages[0], messages[1] },
                MaxTokens = 300,
                Temperature = 0.2f
            };

            var response = await _openAIClient.GetChatCompletionsAsync(options);

            return ParseLlmResponse(response.Value.Choices[0].Message.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM content analysis failed");
            return new LlmResult { SpamScore = 0, Issues = new List<string>() };
        }
    }

    private LlmResult ParseLlmResponse(string response)
    {
        try
        {
            // Parse JSON response from LLM
            // In production, use proper JSON parsing
            var result = new LlmResult
            {
                SpamScore = 50, // Default score
                Issues = new List<string>()
            };

            if (response.Contains("spam", StringComparison.OrdinalIgnoreCase))
                result.Issues.Add("LLM detected spam patterns");

            if (response.Contains("phishing", StringComparison.OrdinalIgnoreCase))
                result.Issues.Add("LLM detected phishing attempt");

            return result;
        }
        catch
        {
            return new LlmResult { SpamScore = 0, Issues = new List<string>() };
        }
    }
}