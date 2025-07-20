using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class ContentGuardWorker : BackgroundService
{
    private readonly PipelineExecutor _pipelineExecutor;

    public ContentGuardWorker(PipelineExecutor pipelineExecutor)
    {
        _pipelineExecutor = pipelineExecutor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Example: Fetch message from RabbitMQ, process
            var request = new ContentAnalysisRequestDTO
            {
                Content = "<html>...</html>",
                ContentType = "html",
                CustomerId = Guid.NewGuid()
            };

            var result = await _pipelineExecutor.ExecuteAsync(request);

            // Save result to DB, log, etc.
            await Task.Delay(1000, stoppingToken);
        }
    }
}