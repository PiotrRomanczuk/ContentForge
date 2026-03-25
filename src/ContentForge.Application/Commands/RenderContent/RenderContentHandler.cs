using ContentForge.Application.DTOs;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.RenderContent;

// IRequestHandler<TCommand, TResult> = the handler that processes a command.
// Like an event listener: mediator.Send(command) finds this handler and calls Handle().
public class RenderContentHandler : IRequestHandler<RenderContentCommand, RenderContentResultDto>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly IMediaRenderer _mediaRenderer;
    private readonly ILogger<RenderContentHandler> _logger;

    public RenderContentHandler(
        IContentItemRepository contentRepository,
        IMediaRenderer mediaRenderer,
        ILogger<RenderContentHandler> logger)
    {
        _contentRepository = contentRepository;
        _mediaRenderer = mediaRenderer;
        _logger = logger;
    }

    public async Task<RenderContentResultDto> Handle(
        RenderContentCommand request, CancellationToken cancellationToken)
    {
        var item = await _contentRepository.GetByIdAsync(request.ContentItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Content item {request.ContentItemId} not found");

        // Guard: only Generated or Queued content can be rendered
        if (item.Status is not (ContentStatus.Generated or ContentStatus.Queued))
            throw new InvalidOperationException(
                $"Content item must be in Generated or Queued status to render. Current: {item.Status}");

        // Pick template: use provided name, or infer from bot name
        var templateName = request.TemplateName
            ?? InferTemplateName(item.BotName, item.ContentType);

        string mediaPath;
        string? thumbnailPath = null;

        if (item.ContentType == ContentType.Carousel)
        {
            // Carousel: parse TextContent as JSON slides, render each separately
            var slides = ParseCarouselSlides(item.TextContent);
            mediaPath = await _mediaRenderer.RenderCarouselAsync(
                slides, templateName, request.Parameters, cancellationToken);
        }
        else
        {
            // Single image: render the full text content as one image
            mediaPath = await _mediaRenderer.RenderImageAsync(
                item.TextContent, templateName, request.Parameters, cancellationToken);
        }

        // Update entity: set media path and advance status
        item.MediaPath = mediaPath;
        item.Status = ContentStatus.Rendered;
        await _contentRepository.UpdateAsync(item, cancellationToken);

        _logger.LogInformation(
            "Rendered content {Id} with template '{Template}' → {Path}",
            item.Id, templateName, mediaPath);

        return new RenderContentResultDto(item.Id, mediaPath, thumbnailPath, templateName);
    }

    // Maps bot names to their default template. Like a switch/case object lookup in JS.
    private static string InferTemplateName(string botName, ContentType contentType) =>
        botName switch
        {
            "EnglishFactsBot" => contentType == ContentType.Carousel
                ? "english-facts-carousel" : "english-facts",
            "HoroscopeBot" => contentType == ContentType.Carousel
                ? "horoscope-carousel" : "horoscope",
            _ => "minimal"
        };

    private static IEnumerable<string> ParseCarouselSlides(string textContent)
    {
        // Carousel TextContent is stored as a JSON array of slide objects.
        // Each slide has "heading" and "body" — combine them for rendering.
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(textContent);
            return doc.RootElement.EnumerateArray()
                .Select(slide =>
                {
                    var heading = slide.GetProperty("heading").GetString() ?? "";
                    var body = slide.GetProperty("body").GetString() ?? "";
                    return $"{heading}\n\n{body}";
                })
                .ToList();
        }
        catch
        {
            // Fallback: treat as a single slide if JSON parsing fails
            return new[] { textContent };
        }
    }
}
