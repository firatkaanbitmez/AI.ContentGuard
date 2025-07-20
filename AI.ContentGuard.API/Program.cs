using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Pipelines;
using AI.ContentGuard.Application.Pipelines.Interfaces;
using AI.ContentGuard.Application.Pipelines.Steps;
using AI.ContentGuard.Application.Services;
using AI.ContentGuard.Infrastructure.AI.ImageAnalysis;
using AI.ContentGuard.Infrastructure.Data;
using AI.ContentGuard.Infrastructure.Messaging;
using AI.ContentGuard.Infrastructure.Repositories;
using AI.ContentGuard.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AI.ContentGuard API", Version = "v1" });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database
builder.Services.AddDbContext<ContentGuardDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cache
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });
    });
});

// Register all services (same as Worker)
RegisterServices(builder.Services);

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ContentGuardDbContext>()
    .AddRabbitMQ(rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"]}:{builder.Configuration["RabbitMQ:Password"]}@{builder.Configuration["RabbitMQ:Host"]}")
    .AddRedis(builder.Configuration.GetConnectionString("Redis"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ContentGuardDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

void RegisterServices(IServiceCollection services)
{
    // Infrastructure Services
    services.AddScoped<ISpamRuleRepository, SpamRuleRepository>();
    services.AddScoped<IAuditLogger, DatabaseAuditLogger>();
    services.AddScoped<IMessagePublisher, MessagePublisher>();

    // Application Services
    services.AddScoped<ITemplateAnalysisService, TemplateAnalysisService>();
    services.AddScoped<IInjectionValidator, InjectionValidator>();
    services.AddScoped<ISpamDetectionEngine, SpamDetectionEngine>();
    services.AddScoped<IRuleEngine, RuleEngine>();
    services.AddScoped<IScoreCalculator, ScoreCalculator>();
    services.AddScoped<IRecommendationEngine, RecommendationEngine>();
    services.AddScoped<IFeedbackHandler, FeedbackHandler>();

    // Image Analysis Services
    services.AddScoped<ITextPresenceDetector, TextPresenceDetector>();
    services.AddScoped<ITesseractOcr, TesseractOcrService>();
    services.AddScoped<IImageHashService, ImageHashService>();
    services.AddScoped<ICnnModel, CnnModelService>();
    services.AddScoped<ILlmService, LlmImageAnalyzer>();
    services.AddScoped<IImageAnalysisPipeline, LayeredImageAnalyzer>();

    // Pipeline Steps
    services.AddScoped<IPipelineStep, NormalizationStep>();
    services.AddScoped<IPipelineStep, InjectionValidationStep>();
    services.AddScoped<IPipelineStep, SpamDetectionStep>();
    services.AddScoped<IPipelineStep, ImageAnalysisStep>();
    services.AddScoped<IPipelineStep, ScoreCalculationStep>();

    // Pipeline
    services.AddScoped<IContentAnalysisPipeline, ContentAnalysisPipelineExecutor>();
}