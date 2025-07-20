namespace AI.ContentGuard.Domain.Entities;

public class CustomerProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RiskThreshold { get; set; }
    public string Email { get; set; } = string.Empty;
}