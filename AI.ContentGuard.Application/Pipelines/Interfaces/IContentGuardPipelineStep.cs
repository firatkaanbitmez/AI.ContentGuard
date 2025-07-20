using AI.ContentGuard.Application.Pipelines;

public interface IContentGuardPipelineStep
{
    Task ProcessAsync(PipelineContext context);
}