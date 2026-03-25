using ContentForge.Domain.Common;

namespace ContentForge.Domain.Entities;

// Database-persisted bot config. Separate from IBotDefinition (which is code-defined).
// Think of IBotDefinition as the class, BotRegistration as the DB row that enables/configures it.
public class BotRegistration : BaseEntity
{
    public string BotName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string CronExpression { get; set; } = "0 */6 * * *"; // every 6 hours
    public Dictionary<string, string> Configuration { get; set; } = new();

    // One-to-many: a bot can have multiple schedule configs (one per social account).
    public ICollection<ScheduleConfig> Schedules { get; set; } = new List<ScheduleConfig>();
}
