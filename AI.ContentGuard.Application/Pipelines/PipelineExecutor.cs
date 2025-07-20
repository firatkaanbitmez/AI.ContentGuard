public class PipelineExecutor
{
    private readonly List<IContentGuardPipelineStep> _steps;

    public PipelineExecutor(IEnumerable<IContentGuardPipelineStep> steps)
    {
        _steps = steps.ToList();
    }

    public async Task<ContentAnalysisResultDTO> ExecuteAsync(ContentAnalysisRequestDTO request)
    {
        var context = new PipelineContext(request);
        foreach (var step in _steps)
        {
            await step.ProcessAsync(context);
        }
        return context.ToResultDTO();
    }
}