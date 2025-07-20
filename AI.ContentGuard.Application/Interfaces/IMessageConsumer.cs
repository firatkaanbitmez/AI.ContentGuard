using AI.ContentGuard.Application.DTOs;


namespace AI.ContentGuard.Application.Interfaces;

public interface IMessageConsumer
{
    Task<ContentAnalysisMessage?> ConsumeAsync(CancellationToken cancellationToken);
}