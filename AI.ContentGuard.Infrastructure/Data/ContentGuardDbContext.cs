using Microsoft.EntityFrameworkCore;
using AI.ContentGuard.Domain.Entities;
using AI.ContentGuard.Application.Interfaces; // Add this

namespace AI.ContentGuard.Infrastructure.Data;

public class ContentGuardDbContext : DbContext, IContentGuardDbContext // Implement interface
{
    public ContentGuardDbContext(DbContextOptions<ContentGuardDbContext> options)
        : base(options) { }

    public DbSet<AnalysisRequest> AnalysisRequests => Set<AnalysisRequest>();
    public DbSet<AnalysisResult> AnalysisResults => Set<AnalysisResult>();
    public DbSet<DetectedIssue> DetectedIssues => Set<DetectedIssue>();
    public DbSet<SpamRule> SpamRules => Set<SpamRule>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<ImageHash> ImageHashes => Set<ImageHash>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
}