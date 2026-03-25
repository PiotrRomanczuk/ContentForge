using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Interfaces.Services;

// Not implemented yet. Will use SixLabors.ImageSharp (like sharp/canvas in Node.js)
// to render text content into social media images/carousels.
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
