using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AI.ContentGuard.Infrastructure.Data;
using RabbitMQ.Client;

namespace AI.ContentGuard.Worker;

public class HealthCheckService : BackgroundService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public HealthCheckService(
        ILogger<HealthCheckService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckSystemHealth();
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task CheckSystemHealth()
    {
        var health = new Dictionary<string, bool>();

        // Check Database
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ContentGuardDbContext>();
            await dbContext.Database.CanConnectAsync();
            health["Database"] = true;
        }
        catch
        {
            health["Database"] = false;
        }

        // Check RabbitMQ
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"],
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };

            using var connection = factory.CreateConnection();
            health["RabbitMQ"] = connection.IsOpen;
        }
        catch
        {
            health["RabbitMQ"] = false;
        }

        // Check Redis
        try
        {
            // Redis health check would go here
            health["Redis"] = true;
        }
        catch
        {
            health["Redis"] = false;
        }

        // Log health status
        var healthStatus = health.All(h => h.Value) ? "Healthy" : "Unhealthy";
        _logger.LogInformation("System Health: {Status} - Details: {Details}",
            healthStatus,
            string.Join(", ", health.Select(h => $"{h.Key}={h.Value}")));
    }
}