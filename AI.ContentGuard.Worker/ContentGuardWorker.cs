using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace AI.ContentGuard.Worker;

public class ContentGuardWorker : BackgroundService
{
    private readonly ILogger<ContentGuardWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ContentGuardWorker(ILogger<ContentGuardWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ContentGuard Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Log worker health
                var process = Process.GetCurrentProcess();
                _logger.LogInformation("Analysis result published for RequestId: {RequestId}", requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish analysis result for RequestId: {RequestId}", requestId);
                throw;
            }
        }
    }

    public class AnalysisCompletedEvent
    {
        public Guid RequestId { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }
    formation("Worker Status - Memory: {memory}MB, Threads: {threads}",
                        process.WorkingSet64 / 1024 / 1024,
                        process.Threads.Count);

    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ContentGuard Worker");
            }
        }

        _logger.LogInformation("ContentGuard Worker stopping at: {time}", DateTimeOffset.Now);
    }
}
