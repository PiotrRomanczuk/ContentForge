namespace ContentForge.Application.DTOs;

public record ScheduleConfigDto(
    Guid Id,
    Guid BotRegistrationId,
    string BotName,
    Guid SocialAccountId,
    string AccountName,
    string CronExpression,
    bool IsActive,
    string PreferredContentType,
    DateTime CreatedAt);

public record CreateScheduleDto(
    Guid BotRegistrationId,
    Guid SocialAccountId,
    string CronExpression,
    string? PreferredContentType = null,
    Dictionary<string, string>? OverrideConfig = null);

public record UpdateScheduleDto(
    string? CronExpression = null,
    bool? IsActive = null,
    string? PreferredContentType = null);

public record JobStatusDto(
    string JobId,
    string JobName,
    string CronExpression,
    string? NextExecution,
    string? LastExecution);
