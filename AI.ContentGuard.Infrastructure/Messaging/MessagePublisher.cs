using MassTransit;
using Microsoft.Extensions.Logging;

namespace AI.ContentGuard.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishAnalysisResultAsync(Guid requestId, int riskScore, string riskLevel);
}

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(IPublishEndpoint publishEndpoint, ILogger<MessagePublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAnalysisResultAsync(Guid requestId, int riskScore, string riskLevel)
    {
        try
        {
            await _publishEndpoint.Publish(new AnalysisCompletedEvent
            {
                RequestId = requestId,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                CompletedAt = DateTime.UtcNow
            });

            _logger.LogIn