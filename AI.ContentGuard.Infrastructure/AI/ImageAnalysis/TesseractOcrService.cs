using AI.ContentGuard.Application.Interfaces;
using Tesseract;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Infrastructure.AI.ImageAnalysis;

public class TesseractOcrService : ITesseractOcr
{
    private readonly ILogger<TesseractOcrService> _logger;
    private readonly string _tessDataPath;

    public TesseractOcrService(ILogger<TesseractOcrService> logger, string tessDataPath = @"./tessdata")
    {
        _logger = logger;
        _tessDataPath = tessDataPath;
    }

    public async Task<string> ExtractText(byte[] imageData)
    {
        try
        {
            using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromMemory(imageData);
            using var page = engine.Process(img);

            var text = page.GetText();
            var confidence = page.GetMeanConfidence();

            _logger.LogDebug("OCR completed. Confidence: {Confidence}, Text length: {Length}",
                confidence, text.Length);

            return await Task.FromResult(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed");
            return string.Empty;
        }
    }
}