using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces
{
    public interface IMessageConsumerHandler
    {
        Task HandleAsync(ContentAnalysisRequestDto request, CancellationToken cancellationToken);
    }
}