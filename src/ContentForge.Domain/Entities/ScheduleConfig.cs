using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

// Links a bot to a social account with a cron schedule — "post EnglishFactsBot to Instagram daily at 9am".
// Like a node-cron job definition stored in the database.
public class ScheduleConfig : BaseEntity
{
    public Guid BotRegistrationId { get; set; }
    public BotRegistration BotRegistration { get; set; } = null!;

    public Guid SocialAccountId { get; set; }
    public SocialAccount SocialAccount { get; set; } = null!;

    // Same cron syntax as node-cron / GitHub Actions. "0 9 * * *" = daily at 9 AM.
    public string CronExpression { get; set; } = "0 9 * * *";
    public bool IsActive { get; set; } = true;
    public ContentType PreferredContentType { get; set; } = ContentType.Image;
    public Dictionary<string, string> OverrideConfig { get; set; } = new();
}
