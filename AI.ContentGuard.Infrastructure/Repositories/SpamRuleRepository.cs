using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.ContentGuard.Infrastructure.Repositories;

public class SpamRuleRepository : ISpamRuleRepository
{
    private readonly ContentGuardDbContext _context;

    public SpamRuleRepository(ContentGuardDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SpamRule>> GetAllRulesAsync()
    {
        var dbRules = await _context.SpamRules
            .Where(r => r.Priority > 0)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        return dbRules.Select(r => new Application.Interfaces.SpamRule
        {
            Id = r.Id,
            Pattern = r.Pattern,
            Priority = r.Priority,
            Score = r.Score
        });
    }
}