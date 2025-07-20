namespace AI.ContentGuard.Application.Interfaces;

public interface ITesseractOcr
{
    Task<string> ExtractText(byte[] imageData);
}