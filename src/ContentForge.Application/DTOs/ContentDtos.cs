using ContentForge.Domain.Enums;

namespace ContentForge.Application.DTOs;

public record ContentItemDto(
    Guid Id,
    string BotName,
    string Category,
    ContentType ContentType,
    ContentStatus Status,
    string TextContent,
    string? MediaPath,
    DateTime? ScheduledAt,
    DateTime? PublishedAt,
    DateTime CreatedAt);

public record ContentBatchResultDto(
    int TotalGenerated,
    int Succeeded,
    int Failed,
    IReadOnlyList<ContentItemDto> Items);

public record BotInfoDto(
    string Name,
    string Category,
    string Description,
    IReadOnlyList<ContentType> SupportedContentTypes);

public record SocialAccountDto(
    Guid Id,
    string Name,
    Platform Platform,
    string ExternalId,
    bool IsActive,
    DateTime? TokenExpiresAt);

public record ContentMetricSummaryDto(
    Guid ContentItemId,
    int TotalImpressions,
    int TotalReach,
    int TotalLikes,
    int TotalComments,
    int TotalShares,
    double AverageEngagementRate);

public record ApprovalDecisionDto(
    Guid ContentItemId,
    bool Approved,
    string? EditedText = null,
    DateTime? RescheduleAt = null);

public record BulkApprovalResultDto(
    int Approved,
    int Rejected,
    int Edited);
