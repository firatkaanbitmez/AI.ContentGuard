using AI.ContentGuard.Application.DTOs;

namespace AI.ContentGuard.Application.Interfaces;

public interface IInjectionValidator
{
    Task<bool> HasInjectionAsync(NormalizedContent content);
}