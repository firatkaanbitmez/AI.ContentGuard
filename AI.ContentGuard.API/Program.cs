using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRecommendationEngine, RecommendationEngine>();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ContentNormalizer>();
builder.Services.AddScoped<LLMProxyService>();
builder.Services.AddScoped<ImageHashChecker>();
builder.Services.AddScoped<TPDService>();
builder.Services.AddScoped<FeedbackTrainer>();
builder.Services.AddScoped<SafeContentRecommender>();

// Register pipeline steps and executor
builder.Services.AddScoped<IContentGuardPipelineStep>();
builder.Services.AddScoped<PipelineExecutor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();




app.Run();

