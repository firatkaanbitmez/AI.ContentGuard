using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Pipelines;
using AI.ContentGuard.Application.Pipelines.Interfaces;
using AI.ContentGuard.Application.Pipelines.Steps;
using AI.ContentGuard.Application.Services;
using AI.ContentGuard.Infrastructure.AI.ImageAnalysis;
using AI.ContentGuard.Infrastructure.Data;
using AI.ContentGuard.Infrastructure.Messaging;
using AI.ContentGuard.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "contentguard-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();


// Database
builder.Services.AddDbContext<ContentGuardDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register as interface
builder.Services.AddScoped<IContentGuardDbContext>(provider =>
    provider.GetRequiredService<ContentGuardDbContext>());
// Cache
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Infrastructure Services
builder.Services.AddScoped<ISpamRuleRepository, SpamRuleRepository>();
builder.Services.AddScoped<IAuditLogger, DatabaseAuditLogger>();

// Application Services
builder.Services.AddScoped<ITemplateAnalysisService, TemplateAnalysisService>();
builder.Services.AddScoped<IInjectionValidator, InjectionValidator>();
builder.Services.AddScoped<ISpamDetectionEngine, SpamDetectionEngine>();
builder.Services.AddScoped<IRuleEngine, RuleEngine>();
builder.Services.AddScoped<IScoreCalculator, ScoreCalculator>();

// Image Analysis Services
builder.Services.AddScoped<ITextPresenceDetector, TextPresenceDetector>();
builder.Services.AddScoped<ITesseractOcr, TesseractOcrService>();
builder.Services.AddScoped<IImageHashService, ImageHashService>();
builder.Services.AddScoped<ICnnModel, CnnModelService>();
builder.Services.AddScoped<ILlmService, LlmImageAnalyzer>();
builder.Services.AddScoped<IImageAnalysisPipeline, LayeredImageAnalyzer>();

// Pipeline Steps
builder.Services.AddScoped<IPipelineStep, NormalizationStep>();
builder.Services.AddScoped<IPipelineStep, InjectionValidationStep>();
builder.Services.AddScoped<IPipelineStep, SpamDetectionStep>();
builder.Services.AddScoped<IPipelineStep, ImageAnalysisStep>();
builder.Services.AddScoped<IPipelineStep, ScoreCalculationStep>();

// Pipeline
builder.Services.AddScoped<IContentAnalysisPipeline, ContentAnalysisPipelineExecutor>();
builder.Services.AddScoped<IMessageConsumerHandler, MessageConsumerHandler>();

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ContentAnalysisRequestConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ReceiveEndpoint(builder.Configuration["RabbitMQ:QueueName"], e =>
        {
            e.PrefetchCount = 16;
            e.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
            e.ConfigureConsumer<ContentAnalysisRequestConsumer>(context);
        });
    });
});

// Background Services
builder.Services.AddHostedService<ContentGuardWorker>();
builder.Services.AddHostedService<HealthCheckService>();

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ContentGuardDbContext>();
    await dbContext.Database.MigrateAsync();
}

await host.RunAsync();