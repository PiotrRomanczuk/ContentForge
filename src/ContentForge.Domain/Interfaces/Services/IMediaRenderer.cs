using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Interfaces.Services;

public interface IMediaRenderer
{
    Task<string> RenderImageAsync(
        string text,
        string templateName,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default);

    Task<string> RenderCarouselAsync(
        IEnumerable<string> slides,
        string templateName,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default);

    IReadOnlyList<string> GetAvailableTemplates(ContentType contentType);
}
