using AI.ContentGuard.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Infrastructure.AI.ImageAnalysis;

public class TextPresenceDetector : ITextPresenceDetector
{
    private readonly ILogger<TextPresenceDetector> _logger;
    private const double EDGE_DENSITY_THRESHOLD = 0.15;
    private const double VARIANCE_THRESHOLD = 500;

    public TextPresenceDetector(ILogger<TextPresenceDetector> logger)
    {
        _logger = logger;
    }

    public async Task<bool> DetectTextPresence(byte[] imageData)
    {
        try
        {
            using var image = Image.Load<Rgba32>(imageData);

            // Convert to grayscale
            image.Mutate(x => x.Grayscale());

            // Apply edge detection
            var edges = ApplySobelOperator(image);

            // Calculate edge density
            var edgeDensity = CalculateEdgeDensity(edges);

            // Calculate variance
            var variance = CalculateVariance(image);

            // Text typically has high edge density and high variance
            var hasText = edgeDensity > EDGE_DENSITY_THRESHOLD || variance > VARIANCE_THRESHOLD;

            _logger.LogDebug("Text presence detection - EdgeDensity: {EdgeDensity}, Variance: {Variance}, HasText: {HasText}",
                edgeDensity, variance, hasText);

            return await Task.FromResult(hasText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting text presence");
            return true; // Assume text present on error to trigger OCR
        }
    }

    private float[,] ApplySobelOperator(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;
        var edges = new float[width, height];

        var sobelX = new[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        var sobelY = new[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float gx = 0, gy = 0;

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        var pixel = image[x + j, y + i];
                        var intensity = pixel.R; // Grayscale, so R=G=B

                        gx += intensity * sobelX[i + 1, j + 1];
                        gy += intensity * sobelY[i + 1, j + 1];
                    }
                }

                edges[x, y] = MathF.Sqrt(gx * gx + gy * gy);
            }
        }

        return edges;
    }

    private double CalculateEdgeDensity(float[,] edges)
    {
        var threshold = 50f;
        var edgePixels = 0;
        var totalPixels = edges.GetLength(0) * edges.GetLength(1);

        for (int x = 0; x < edges.GetLength(0); x++)
        {
            for (int y = 0; y < edges.GetLength(1); y++)
            {
                if (edges[x, y] > threshold)
                    edgePixels++;
            }
        }

        return (double)edgePixels / totalPixels;
    }

    private double CalculateVariance(Image<Rgba32> image)
    {
        double sum = 0, sumSquared = 0;
        var pixelCount = image.Width * image.Height;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                var intensity = pixel.R; // Grayscale
                sum += intensity;
                sumSquared += intensity * intensity;
            }
        }

        var mean = sum / pixelCount;
        return (sumSquared / pixelCount) - (mean * mean);
    }
}