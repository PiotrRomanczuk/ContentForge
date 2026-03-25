using ContentForge.Application.DTOs;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.ContentGraph;

// Handler for ExtractEntitiesCommand — extracts entities from content text,
// generates an embedding, and stores both for graph queries.
// IRequestHandler<TCommand, TResult> = MediatR picks this up automatically via DI.
// Like an Express route handler, but decoupled from the HTTP layer.
public class ExtractEntitiesHandler : IRequestHandler<ExtractEntitiesCommand, EntityExtractionResultDto>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly IContentGraphService _graphService;
    private readonly ILogger<ExtractEntitiesHandler> _logger;

    public ExtractEntitiesHandler(
        IContentItemRepository contentRepository,
        IContentGraphService graphService,
        ILogger<ExtractEntitiesHandler> logger)
    {
        _contentRepository = contentRepository;
        _graphService = graphService;
        _logger = logger;
    }

    public async Task<EntityExtractionResultDto> Handle(
        ExtractEntitiesCommand request, CancellationToken cancellationToken)
    {
        var item = await _contentRepository.GetByIdAsync(request.ContentItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Content item {request.ContentItemId} not found");

        _logger.LogInformation("Extracting entities from content {Id} ({BotName})",
            item.Id, item.BotName);

        // Step 1: Extract entities with salience scores from the text content.
        var entities = await _graphService.ExtractEntitiesAsync(item.Id, cancellationToken);

        // Step 2: Generate and store the vector embedding for KNN search.
        await _graphService.GenerateEmbeddingAsync(item.Id, cancellationToken);

        _logger.LogInformation("Extracted {Count} entities from content {Id}",
            entities.Count, item.Id);

        return new EntityExtractionResultDto(
            item.Id,
            entities.Count,
            entities.Select(e => new ContentEntityDto(
                e.Id, e.ContentItemId, e.Name, e.EntityType,
                e.SalienceScore, e.MentionCount)).ToList());
    }
}
