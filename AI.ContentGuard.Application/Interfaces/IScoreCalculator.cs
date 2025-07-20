using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces;

public interface IScoreCalculator
{
    int CalculateRiskScore(SpamDetectionResult spamResult, bool hasInjection, ImageAnalysisResult imageResult);
    string GetRiskLevel(int score);
}