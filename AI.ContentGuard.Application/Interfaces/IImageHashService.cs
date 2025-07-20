namespace AI.ContentGuard.Application.Interfaces;

public class ImageHashResult
{
    public bool IsBlacklisted { get; set; }
    public bool IsWhitelisted { get; set; }
}

public interface IImageHashService
{
    Task<ImageHashResult> CheckImageHash(byte[] imageData);
}