public interface IContentGuardPipelineStep
{
    Task ProcessAsync(PipelineContext context);
}