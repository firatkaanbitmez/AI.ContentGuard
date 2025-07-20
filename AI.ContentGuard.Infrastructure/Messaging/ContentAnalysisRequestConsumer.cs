using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Infrastructure.Messaging;

public class ContentAnalysisRequestConsumer : IConsumer<ContentAnalysisRequestDto>
{
    private readonly ILogger<ContentAnalysisRequestConsumer> _logger;
    private readonly IMessageConsumerHandler _handler;

    public ContentAnalysisRequestConsumer(ILogger<ContentAnalysisRequestConsumer> logger, IMessageConsumerHandler handler)
    {
        _logger = logger;
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<ContentAnalysisRequestDto> context)
    {
        _logger.LogInformation("Received content analysis request: {ContentType}", context.Message.ContentType);
        await _handler.HandleAsync(context.Message, context.CancellationToken);
    }
}