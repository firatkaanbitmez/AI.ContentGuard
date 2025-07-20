using Microsoft.AspNetCore.Mvc;
using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Domain.Entities;
using AI.ContentGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace AI.ContentGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContentAnalysisController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ContentGuardDbContext _dbContext;
    private readonly IFeedbackHandler _feedbackHandler;
    private readonly ILogger<ContentAnalysisController> _logger;

    public ContentAnalysisController(
        IPublishEndpoint publishEndpoint,
        ContentGuardDbContext dbContext,
        IFeedbackHandler feedbackHandler,
        ILogger<ContentAnalysisController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _feedbackHandler = feedbackHandler;
        _logger = logger;
    }

    /// <summary>
    /// Submit content for analysis
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalysisSubmissionResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Analyze([FromBody] ContentAnalysisRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { error = "Content cannot be empty" });
        }

        var validContentTypes = new[] { "html", "json", "plain", "text", "image" };
        if (!validContentTypes.Contains(request.ContentType.ToLower()))
        {
            return BadRequest(new { error = "Invalid content type. Supported types: html, json, plain, text, image" });
        }

        try
        {
            var requestId = Guid.NewGuid();

            // Save request to database
            var analysisRequest = new AnalysisRequest
            {
                Id = requestId,
                ContentType = request.ContentType,
                Content = request.Content,
                CustomerProfile = "default", // In production, get from auth
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.AnalysisRequests.Add(analysisRequest);
            await _dbContext.SaveChangesAsync();

            // Publish to RabbitMQ
            await _publishEndpoint.Publish(new ContentAnalysisRequestDto
            {
                ContentType = request.ContentType,
                Content = request.Content
            });

            _logger.LogInformation("Analysis request submitted: {RequestId}", requestId);

            return Ok(new AnalysisSubmissionResponse
            {
                RequestId = requestId,
                Status = "Processing",
                Message = "Content analysis request has been queued for processing"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting analysis request");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get analysis status and results
    /// </summary>
    [HttpGet("status/{requestId}")]
    [ProducesResponseType(typeof(AnalysisStatusResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetStatus(Guid requestId)
    {
        var request = await _dbContext.AnalysisRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
        {
            return NotFound(new { error = "Request not found" });
        }

        var result = await _dbContext.AnalysisResults
            .Include(r => r.Issues)
            .FirstOrDefaultAsync(r => r.RequestId == requestId);

        if (result == null)
        {
            return Ok(new AnalysisStatusResponse
            {
                RequestId = requestId,
                Status = "Processing",
                CreatedAt = request.CreatedAt
            });
        }

        return Ok(new AnalysisStatusResponse
        {
            RequestId = requestId,
            Status = "Completed",
            CreatedAt = request.CreatedAt,
            CompletedAt = result.CompletedAt,
            Result = new ContentAnalysisResult
            {
                RequestId = requestId,
                RiskScore = result.RiskScore,
                RiskLevel = result.RiskLevel,
                Issues = result.Issues.Select(i => i.Description).ToList()
            }
        });
    }

    /// <summary>
    /// Submit feedback for analysis results
    /// </summary>
    [HttpPost("feedback")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackDto feedback)
    {
        if (feedback.RequestId == Guid.Empty)
        {
            return BadRequest(new { error = "Invalid request ID" });
        }

        try
        {
            await _feedbackHandler.HandleFeedbackAsync(
                feedback.RequestId,
                feedback.IsFalsePositive,
                feedback.IsFalseNegative);

            return Ok(new { message = "Feedback received and will be used to improve our models" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing feedback");
            return StatusCode(500, new { error = "Failed to process feedback" });
        }
    }

    /// <summary>
    /// Get analysis statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AnalysisStatistics), 200)]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;

        var stats = await _dbContext.AnalysisResults
            .Where(r => r.CompletedAt >= fromDate && r.CompletedAt <= toDate)
            .GroupBy(r => r.RiskLevel)
            .Select(g => new { RiskLevel = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalRequests = await _dbContext.AnalysisRequests
            .CountAsync(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate);

        var avgProcessingTime = await _dbContext.AnalysisResults
            .Where(r => r.CompletedAt >= fromDate && r.CompletedAt <= toDate)
            .Select(r => EF.Functions.DateDiffSecond(
                _dbContext.AnalysisRequests.First(req => req.Id == r.RequestId).CreatedAt,
                r.CompletedAt))
            .AverageAsync();

        return Ok(new AnalysisStatistics
        {
            TotalRequests = totalRequests,
            RiskLevelDistribution = stats.ToDictionary(s => s.RiskLevel, s => s.Count),
            AverageProcessingTimeSeconds = avgProcessingTime ?? 0,
            Period = new { From = fromDate, To = toDate }
        });
    }
}