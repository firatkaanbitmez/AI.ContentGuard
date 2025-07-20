namespace AI.ContentGuard.Application.DTOs;

public class ImageMetadata
{
    public bool IsValidFormat { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
}