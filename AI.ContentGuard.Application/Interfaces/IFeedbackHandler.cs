namespace AI.ContentGuard.Application.Interfaces;

public interface IFeedbackHandler
{
    Task HandleFeedbackAsync(Guid requestId, bool isFalsePositive, bool isFalseNegative);
}