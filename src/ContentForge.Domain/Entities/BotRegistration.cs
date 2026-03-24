using ContentForge.Domain.Common;

namespace ContentForge.Domain.Entities;

public class BotRegistration : BaseEntity
{
    public string BotName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string CronExpression { get; set; } = "0 */6 * * *"; // every 6h default
    public Dictionary<string, string> Configuration { get; set; } = new();

    public ICollection<ScheduleConfig> Schedules { get; set; } = new List<ScheduleConfig>();
}
