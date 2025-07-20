using AI.ContentGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AI.ContentGuard.Application.Interfaces;

public interface IContentGuardDbContext
{
    DbSet<AnalysisRequest> AnalysisRequests { get; }
    DbSet<AnalysisResult> AnalysisResults { get; }
    DbSet<DetectedIssue> DetectedIssues { get; }
    DbSet<SpamRule> SpamRules { get; }
    DbSet<CustomerProfile> CustomerProfiles { get; }
    DbSet<ImageHash> ImageHashes { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}