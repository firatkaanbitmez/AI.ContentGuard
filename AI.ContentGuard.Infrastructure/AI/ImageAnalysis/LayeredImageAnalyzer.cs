using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Domain.Entities; // Add this for ImageAnalysisResult
using Microsoft.Extensions.Logging;

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
        // Layer 0: Metadata Check
        var metadata = await AnalyzeMetadata(imageData);
        if (!metadata.IsValidFormat)
            return new ImageAnalysisResult { Issues = new List<string> { "Invalid image format" } };

        // Layer 1: OCR and Hash Check
        var hasText = await _textDetector.DetectTextPresence(imageData);
        var hashResult = await _hashService.CheckImageHash(imageData);
        
        if (hashResult.IsBlacklisted)
            return new ImageAnalysisResult { Issues = new List<string> { "Blacklisted image" } };

        // Layer 2: CNN Analysis
        var cnnResult = await _cnnModel.Analyze(imageData);
        if (cnnResult.NsfwProbability > 0.8)
            return new ImageAnalysisResult { Issues = new List<string> { "NSFW content detected" } };

        // Layer 3: LLM Analysis (only if needed)
        if (cnnResult.RequiresDetailedAnalysis)
        {
            var llmResult = await _llmService.AnalyzeImage(imageData);
            return CombineResults(cnnResult, llmResult);
        }

        return new ImageAnalysisResult
        {
            Issues = cnnResult.DetectedIssues
        };
    }

    private Task<ImageMetadata> AnalyzeMetadata(byte[] imageData) => throw new NotImplementedException();
    private ImageAnalysisResult CombineResults(CnnResult cnn, LlmResult llm) => throw new NotImplementedException();
}   