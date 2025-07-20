using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Infrastructure.AI.ImageAnalysis;

public class LayeredImageAnalyzer : IImageAnalysisPipeline
{
    private readonly ILogger<LayeredImageAnalyzer> _logger;
    private readonly ITextPresenceDetector _textDetector;
    private readonly ITesseractOcr _ocr;
    private readonly ICnnModel _cnnModel;
    private readonly ILlmService _llmService;
    private readonly IImageHashService _hashService;

    public LayeredImageAnalyzer(
        ILogger<LayeredImageAnalyzer> logger,
        ITextPresenceDetector textDetector,
        ITesseractOcr ocr,
        ICnnModel cnnModel,
        ILlmService llmService,
        IImageHashService hashService)
    {
        _logger = logger;
        _textDetector = textDetector;
        _ocr = ocr;
        _cnnModel = cnnModel;
        _llmService = llmService;
        _hashService = hashService;
    }

    public async Task<ImageAnalysisResult> AnalyzeAsync(byte[] imageData)
    {
        try
        {
            // Layer 0: Metadata Check
            var metadata = await AnalyzeMetadata(imageData);
            if (!metadata.IsValidFormat)
                return new ImageAnalysisResult { Issues = new List<string> { "Invalid image format" } };

            // Layer 1: OCR and Hash Check
            var hasText = await _textDetector.DetectTextPresence(imageData);
            var hashResult = await _hashService.CheckImageHash(imageData);

            if (hashResult.IsBlacklisted)
                return new ImageAnalysisResult
                {
                    IsSpam = true,
                    Issues = new List<string> { "Blacklisted image" }
                };

            // Layer 2: CNN Analysis
            var cnnResult = await _cnnModel.Analyze(imageData);
            if (cnnResult.NsfwProbability > 0.8)
                return new ImageAnalysisResult
                {
                    IsNSFW = true,
                    Issues = new List<string> { "NSFW content detected" }
                };

            // Layer 3: LLM Analysis (only if needed)
            if (cnnResult.RequiresDetailedAnalysis)
            {
                var llmResult = await _llmService.AnalyzeImage(imageData);
                return CombineResults(cnnResult, llmResult);
            }

            return new ImageAnalysisResult
            {
                Issues = cnnResult.DetectedIssues,
                IsNSFW = cnnResult.NsfwProbability > 0.5,
                IsSpam = cnnResult.RiskScore > 70
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in layered image analysis");
            return new ImageAnalysisResult
            {
                Issues = new List<string> { "Image analysis failed" }
            };
        }
    }

    private async Task<ImageMetadata> AnalyzeMetadata(byte[] imageData)
    {
        try
        {
            // Basic format validation
            if (imageData == null || imageData.Length == 0)
                return new ImageMetadata { IsValidFormat = false };

            // Check for common image format headers
            var isValidFormat = IsValidImageFormat(imageData);

            return await Task.FromResult(new ImageMetadata
            {
                IsValidFormat = isValidFormat,
                Width = 0, // Would be extracted in real implementation
                Height = 0,
                Format = GetImageFormat(imageData)
            });
        }
        catch
        {
            return new ImageMetadata { IsValidFormat = false };
        }
    }

    private ImageAnalysisResult CombineResults(CnnResult cnn, LlmResult llm)
    {
        var combinedIssues = new List<string>();
        combinedIssues.AddRange(cnn.DetectedIssues);
        combinedIssues.AddRange(llm.Issues);

        return new ImageAnalysisResult
        {
            IsSpam = llm.SpamScore > 50 || cnn.RiskScore > 50,
            IsNSFW = cnn.NsfwProbability > 0.5,
            IsManipulated = combinedIssues.Any(i => i.ToLower().Contains("manipulated")),
            Issues = combinedIssues
        };
    }

    private bool IsValidImageFormat(byte[] imageData)
    {
        if (imageData.Length < 4) return false;

        // Check for common image format signatures
        // JPEG: FF D8 FF
        if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
            return true;

        // PNG: 89 50 4E 47
        if (imageData[0] == 0x89 && imageData[1] == 0x50 &&
            imageData[2] == 0x4E && imageData[3] == 0x47)
            return true;

        // GIF: 47 49 46 38
        if (imageData[0] == 0x47 && imageData[1] == 0x49 &&
            imageData[2] == 0x46 && imageData[3] == 0x38)
            return true;

        return false;
    }

    private string GetImageFormat(byte[] imageData)
    {
        if (imageData.Length < 4) return "unknown";

        if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
            return "jpeg";

        if (imageData[0] == 0x89 && imageData[1] == 0x50 &&
            imageData[2] == 0x4E && imageData[3] == 0x47)
            return "png";

        if (imageData[0] == 0x47 && imageData[1] == 0x49 &&
            imageData[2] == 0x46 && imageData[3] == 0x38)
            return "gif";

        return "unknown";
    }
}