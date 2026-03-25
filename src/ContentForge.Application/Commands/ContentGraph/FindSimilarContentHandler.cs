using ContentForge.Application.DTOs;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.ContentGraph;

// Handler for KNN similarity search — finds the K most similar content items.
// Uses vector embeddings stored in pgvector for fast cosine distance queries.
public class FindSimilarContentHandler : IRequestHandler<FindSimilarContentQuery, SimilaritySearchResultDto>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly IContentGraphService _graphService;
    private readonly ILogger<FindSimilarContentHandler> _logger;

    public FindSimilarContentHandler(
        IContentItemRepository contentRepository,
        IContentGraphService graphService,
        ILogger<FindSimilarContentHandler> logger)
    {
        _contentRepository = contentRepository;
        _graphService = graphService;
        _logger = logger;
    }

    public async Task<SimilaritySearchResultDto> Handle(
        FindSimilarContentQuery request, CancellationToken cancellationToken)
    {
        var item = await _contentRepository.GetByIdAsync(request.ContentItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Content item {request.ContentItemId} not found");

        _logger.LogInformation("Finding {K} similar items for content {Id}", request.K, item.Id);

        var results = await _graphService.FindSimilarContentAsync(
            item.Id, request.K, cancellationToken);

        // Hydrate results with content details for the response.
        var similarItems = new List<SimilarContentDto>();
        foreach (var (contentItemId, score) in results)
        {
            var similar = await _contentRepository.GetByIdAsync(contentItemId, cancellationToken);
            if (similar is null) continue;

            similarItems.Add(new SimilarContentDto(
                similar.Id,
                similar.BotName,
                similar.Category,
                // Truncate text for preview — like .substring(0, 200) in JS.
                similar.TextContent.Length > 200
                    ? similar.TextContent[..200] + "..."
                    : similar.TextContent,
                score));
        }

        return new SimilaritySearchResultDto(item.Id, similarItems.Count, similarItems);
    }
}
