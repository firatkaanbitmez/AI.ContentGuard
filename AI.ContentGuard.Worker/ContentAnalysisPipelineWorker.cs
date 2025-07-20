using AI.ContentGuard.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ContentAnalysisPipelineWorker : BackgroundService
{
    private readonly ILogger<ContentAnalysisPipelineWorker> _logger;
    private readonly IMessageConsumer _messageConsumer;
    private readonly IContentAnalysisPipeline _pipeline;
    private readonly IAuditLogger _auditLogger;

    public ContentAnalysisPipelineWorker(
        ILogger<ContentAnalysisPipelineWorker> logger,
        IMessageConsumer messageConsumer,
        IContentAnalysisPipeline pipeline,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _messageConsumer = messageConsumer;
        _pipeline = pipeline;
        _auditLogger = auditLogger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Content Analysis Worker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await _messageConsumer.ConsumeAsync(stoppingToken);
            if (request != null)
            {
                var result = await _pipeline.ProcessAsync(request, stoppingToken);
                await _auditLogger.LogAsync("AnalysisCompleted", result);
            }
            await Task.Delay(100, stoppingToken);
        }
    }
}