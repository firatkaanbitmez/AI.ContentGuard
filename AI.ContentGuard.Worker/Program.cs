using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Services;
using AI.ContentGuard.Infrastructure.Messaging;
using MassTransit;
using AI.ContentGuard.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ContentAnalysisPipelineWorker>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ContentAnalysisRequestConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => { });
        cfg.ReceiveEndpoint("content_analysis_queue", e =>
        {
            e.ConfigureConsumer<ContentAnalysisRequestConsumer>(context);
        });
    });
});

// Register other services (TemplateAnalysisService, SpamDetectionEngine, etc.)

var host = builder.Build();
host.Run();
