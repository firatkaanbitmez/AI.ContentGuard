namespace AI.ContentGuard.Domain.Entities;

public class ImageHash
{
    public int Id { get; set; }
    public string Hash { get; set; } = string.Empty;
    public bool IsBlacklisted { get; set; }
    public bool IsWhitelisted { get; set; }
    public DateTime CreatedAt { get; set; }
}