using AI.ContentGuard.Domain.Entities;
using AI.ContentGuard.Infrastructure.Data; // Add this using
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Add this using
using Microsoft.Extensions.Caching.Memory;

namespace AI.ContentGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly ContentGuardDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ContentGuardDbContext dbContext,
        IMemoryCache cache,
        ILogger<AdminController> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get all spam rules
    /// </summary>
    [HttpGet("rules")]
    public async Task<IActionResult> GetRules()
    {
        var rules = await _dbContext.SpamRules
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        return Ok(rules);
    }

    /// <summary>
    /// Add new spam rule
    /// </summary>
    [HttpPost("rules")]
    public async Task<IActionResult> AddRule([FromBody] SpamRule rule)
    {
        _dbContext.SpamRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        // Clear cache
        _cache.Remove("spam_rules");

        return Ok(new { message = "Rule added successfully", ruleId = rule.Id });
    }

    /// <summary>
    /// Update spam rule
    /// </summary>
    [HttpPut("rules/{id}")]
    public async Task<IActionResult> UpdateRule(int id, [FromBody] SpamRule rule)
    {
        var existingRule = await _dbContext.SpamRules.FindAsync(id);
        if (existingRule == null)
        {
            return NotFound();
        }

        existingRule.Pattern = rule.Pattern;
        existingRule.Priority = rule.Priority;
        existingRule.Score = rule.Score;

        await _dbContext.SaveChangesAsync();

        // Clear cache
        _cache.Remove("spam_rules");

        return Ok(new { message = "Rule updated successfully" });
    }

    /// <summary>
    /// Delete spam rule
    /// </summary>
    [HttpDelete("rules/{id}")]
    public async Task<IActionResult> DeleteRule(int id)
    {
        var rule = await _dbContext.SpamRules.FindAsync(id);
        if (rule == null)
        {
            return NotFound();
        }

        _dbContext.SpamRules.Remove(rule);
        await _dbContext.SaveChangesAsync();

        // Clear cache
        _cache.Remove("spam_rules");

        return Ok(new { message = "Rule deleted successfully" });
    }

    /// <summary>
    /// Get blacklisted image hashes
    /// </summary>
    [HttpGet("image-blacklist")]
    public async Task<IActionResult> GetImageBlacklist()
    {
        var blacklist = await _dbContext.ImageHashes
            .Where(h => h.IsBlacklisted)
            .OrderByDescending(h => h.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(blacklist);
    }

    /// <summary>
    /// Add image to blacklist
    /// </summary>
    [HttpPost("image-blacklist")]
    public async Task<IActionResult> AddToImageBlacklist([FromBody] ImageHashDto dto)
    {
        var imageHash = new ImageHash
        {
            Hash = dto.Hash,
            IsBlacklisted = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ImageHashes.Add(imageHash);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Image hash added to blacklist" });
    }
}

public class ImageHashDto
{
    public string Hash { get; set; } = string.Empty;
}