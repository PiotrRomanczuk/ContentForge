using ContentForge.Domain.Enums;

namespace ContentForge.Application.DTOs;

// `record` = immutable data class. Like Object.freeze({ id, botName, ... }) in JS.
// Records auto-generate equals, hashCode, and toString — perfect for DTOs.
// The constructor syntax here is shorthand: each parameter becomes a public property.
// Equivalent TS: type ContentItemDto = Readonly<{ id: string; botName: string; ... }>
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
