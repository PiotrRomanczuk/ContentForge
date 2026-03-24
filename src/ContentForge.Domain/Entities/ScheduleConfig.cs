using ContentForge.Domain.Common;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Entities;

public class ScheduleConfig : BaseEntity
{
    public Guid BotRegistrationId { get; set; }
    public BotRegistration BotRegistration { get; set; } = null!;

    public Guid SocialAccountId { get; set; }
    public SocialAccount SocialAccount { get; set; } = null!;

    public string CronExpression { get; set; } = "0 9 * * *"; // daily at 9 AM default
    public bool IsActive { get; set; } = true;
    public ContentType PreferredContentType { get; set; } = ContentType.Image;
    public Dictionary<string, string> OverrideConfig { get; set; } = new();
}
