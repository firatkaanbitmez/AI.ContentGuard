namespace AI.ContentGuard.Application.Interfaces;

public interface ITextPresenceDetector
{
    Task<bool> DetectTextPresence(byte[] imageData);
}