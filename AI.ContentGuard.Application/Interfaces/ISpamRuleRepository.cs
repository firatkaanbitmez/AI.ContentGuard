namespace AI.ContentGuard.Application.Interfaces
{
    public interface ISpamRuleRepository
    {
        Task<IEnumerable<SpamRule>> GetAllRulesAsync();
    }

    public class SpamRule
    {
        public int Id { get; set; }
        public string Pattern { get; set; } = string.Empty;
        public int Priority { get; set; }
        public int Score { get; set; }
    }
}