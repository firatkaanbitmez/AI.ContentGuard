using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces;

public interface IImageAnalysisPipeline
{
    Task<ImageAnalysisResult> AnalyzeAsync(byte[] imageData);
}