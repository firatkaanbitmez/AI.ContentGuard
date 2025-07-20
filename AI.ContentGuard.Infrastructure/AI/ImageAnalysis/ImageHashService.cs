using AI.ContentGuard.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Infrastructure.AI.ImageAnalysis;

public class ImageHashService : IImageHashService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ImageHashService> _logger;

    // In production, these would come from database
    private readonly HashSet<string> _blacklistedHashes = new()
    {
        "1234567890abcdef", // Example blacklisted hashes
        "fedcba0987654321"
    };

    public ImageHashService(IMemoryCache cache, ILogger<ImageHashService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<ImageHashResult> CheckImageHash(byte[] imageData)
    {
        try
        {
            // Generate multiple hash types for better matching
            var md5Hash = GenerateMD5Hash(imageData);
            var perceptualHash = await GeneratePerceptualHash(imageData);

            var result = new ImageHashResult
            {
                IsBlacklisted = _blacklistedHashes.Contains(md5Hash) ||
                               _blacklistedHashes.Contains(perceptualHash),
                IsWhitelisted = false // Check whitelist in production
            };

            _logger.LogDebug("Image hash check - MD5: {MD5}, PHash: {PHash}, Blacklisted: {IsBlacklisted}",
                md5Hash, perceptualHash, result.IsBlacklisted);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking image hash");
            return new ImageHashResult { IsBlacklisted = false, IsWhitelisted = false };
        }
    }

    private string GenerateMD5Hash(byte[] data)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private async Task<string> GeneratePerceptualHash(byte[] imageData)
    {
        using var image = Image.Load<Rgba32>(imageData);

        // Resize to 8x8
        image.Mutate(x => x.Resize(8, 8).Grayscale());

        // Calculate average pixel value
        var pixels = new List<byte>();
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                pixels.Add(image[x, y].R);
            }
        }

        var average = pixels.Average(p => p);

        // Generate hash based on whether pixel is above/below average
        var hash = 0UL;
        for (int i = 0; i < 64; i++)
        {
            if (pixels[i] > average)
                hash |= (1UL << i);
        }

        return await Task.FromResult(hash.ToString("X16"));
    }
}