// AI.ContentGuard.Application/Pipelines/Steps/02_InjectionValidationStep.cs
using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Pipelines.Interfaces;
using Microsoft.Extensions.Logging;
using DomainDetectedIssue = AI.ContentGuard.Domain.Entities.DetectedIssue; // Use alias

namespace AI.ContentGuard.Application.Pipelines.Steps;

public class InjectionValidationStep : IPipelineStep
{
    private readonly IInjectionValidator _injectionValidator;
    private readonly ILogger<InjectionValidationStep> _logger;

    public string Name => "Injection Validation";
    public int Order => 2;

    public InjectionValidationStep(IInjectionValidator injectionValidator, ILogger<InjectionValidationStep> logger)
    {
        _injectionValidator = injectionValidator;
        _logger = logger;
    }

    public async Task<PipelineStepResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        try
        {
            if (context.NormalizedContent == null)
                return new PipelineStepResult { Success = true };

            context.HasInjection = await _injectionValidator.HasInjectionAsync(context.NormalizedContent);

            if (context.HasInjection)
            {
                context.Issues.Add(new DomainDetectedIssue
                {
                    Type = "INJECTION_ATTACK",
                    Description = "Potential injection attack detected",
                    Severity = 9
                });
                _logger.LogWarning("Injection detected for RequestId: {RequestId}", context.RequestId);
            }

            return new PipelineStepResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Injection validation failed for RequestId: {RequestId}", context.RequestId);
            return new PipelineStepResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public bool ShouldExecute(PipelineContext context) =>
        context.NormalizedContent != null;
}



