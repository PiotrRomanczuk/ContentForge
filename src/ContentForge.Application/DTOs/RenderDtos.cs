namespace ContentForge.Application.DTOs;

// record = immutable data class, like Object.freeze({ ... }) in JS.
// Records auto-generate Equals, GetHashCode, and ToString based on all properties.

/// <summary>Result returned after successfully rendering a content item.</summary>
public record RenderContentResultDto(
    Guid ContentItemId,
    string MediaPath,
    string? ThumbnailPath,
    string TemplateName);

/// <summary>Incoming request to render a content item.</summary>
public record RenderRequestDto(
    string? TemplateName = null,
    Dictionary<string, string>? Parameters = null);

/// <summary>Describes an available rendering template.</summary>
public record AvailableTemplateDto(
    string Name,
    string Description,
    IReadOnlyList<string> SupportedContentTypes);
