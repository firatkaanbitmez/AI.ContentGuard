using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Application.Services;

public class MessageConsumerHandler : IMessageConsumerHandler
{
    private readonly IContentAnalysisPipeline _pipeline;
    private readonly ILogger<MessageConsumerHandler> _logger;

    public MessageConsumerHandler(
        IContentAnalysisPipeline pipeline,
        ILogger<MessageConsumerHandler> logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    public async Task HandleAsync(ContentAnalysisRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing content analysis request");

            var message = new ContentAnalysisMessage
            {
                RequestId = Guid.NewGuid(),
                ContentType = request.ContentType,
                Content = request.Content
            };

            await _pipeline.ProcessAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling content analysis request");
            throw;
        }
    }
}