namespace ContentForge.Application.DTOs;

public record CreateSocialAccountDto(
    string Name,
    string Platform,
    string ExternalId,
    string AccessToken,
    DateTime? TokenExpiresAt = null,
    Dictionary<string, string>? Metadata = null);

public record UpdateSocialAccountDto(
    string? Name = null,
    string? AccessToken = null,
    DateTime? TokenExpiresAt = null,
    bool? IsActive = null);
