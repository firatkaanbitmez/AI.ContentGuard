using Microsoft.EntityFrameworkCore;
using AI.ContentGuard.Domain.Entities;
using AI.ContentGuard.Application.Interfaces;

namespace AI.ContentGuard.Infrastructure.Db;

public class ContentGuardDbContext : DbContext
{
    public DbSet<AnalysisRequest> AnalysisRequests => Set<AnalysisRequest>();
    public DbSet<AnalysisResult> AnalysisResults => Set<AnalysisResult>();

    public ContentGuardDbContext(DbContextOptions<ContentGuardDbContext> options)
        : base(options) { }
}