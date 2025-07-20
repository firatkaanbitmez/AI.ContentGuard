using AI.ContentGuard.Application.Interfaces;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Application.Services;

public class FeedbackHandler : IFeedbackHandler
{
    private readonly ContentGuardDbContext _dbContext;
    private readonly ILogger<FeedbackHandler> _logger;

    public FeedbackHandler(ContentGuardDbContext dbContext, ILogger<FeedbackHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleFeedbackAsync(Guid requestId, bool isFalsePositive, bool isFalseNegative)
    {
        try
        {
            var result = await _dbContext.AnalysisResults
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (result == null)
            {
                _logger.LogWarning("Feedback received for non-existent request: {RequestId}", requestId);
                return;
            }

            // Update metadata with feedback
            result.AnalysisMetadata["feedback"] = new
            {
                isFalsePositive,
                isFalseNegative,
                receivedAt = DateTime.UtcNow
            };

            await _dbContext.SaveChangesAsync();

            // In production, this would trigger model retraining
            if (isFalsePositive || isFalseNegative)
            {
                _logger.LogInformation("Feedback received for RequestId: {RequestId} - FP: {FP}, FN: {FN}",
                    requestId, isFalsePositive, isFalseNegative);

                // Queue for model retraining
                // await _retrainingQueue.EnqueueAsync(requestId, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling feedback for RequestId: {RequestId}", requestId);
            throw;
        }
    }
}