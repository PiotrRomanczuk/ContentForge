namespace ContentForge.Application.DTOs;

public record PublishContentResultDto(
    Guid ContentItemId,
    bool IsSuccess,
    string? ExternalPostId,
    string? ErrorMessage,
    DateTime AttemptedAt);

public record PublishRequestDto(Guid SocialAccountId);

public record ValidateAccountResultDto(
    Guid AccountId,
    bool IsValid,
    string? ErrorMessage,
    DateTime? TokenExpiresAt);
