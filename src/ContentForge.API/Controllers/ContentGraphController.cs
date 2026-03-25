using ContentForge.Application.Commands.ContentGraph;
using ContentForge.Application.DTOs;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentForge.API.Controllers;

// Controller for the content knowledge graph — entity extraction, similarity search, and clustering.
// Exposes three techniques: entity anchoring, KNN vector search, salience-weighted Louvain.
// [Authorize] = all endpoints require JWT (like Express auth middleware on the router).
[ApiController]
[Route("api/content-graph")]
[Authorize]
public class ContentGraphController : ControllerBase
{
    // IMediator = MediatR dispatcher. Like an event bus: Send(command) → handler → result.
    // Keeps the controller thin — no business logic here, just HTTP ↔ command mapping.
    private readonly IMediator _mediator;
    private readonly IContentEntityRepository _entityRepository;
    private readonly IContentClusterRepository _clusterRepository;
    private readonly ILogger<ContentGraphController> _logger;

    public ContentGraphController(
        IMediator mediator,
        IContentEntityRepository entityRepository,
        IContentClusterRepository clusterRepository,
        ILogger<ContentGraphController> logger)
    {
        _mediator = mediator;
        _entityRepository = entityRepository;
        _clusterRepository = clusterRepository;
        _logger = logger;
    }

    /// <summary>
    /// Extract named entities from a content item's text.
    /// Also generates a vector embedding for KNN similarity search.
    /// </summary>
    // POST = side effect (creates entities + embedding). Like a POST route in Express.
    [HttpPost("{id:guid}/extract")]
    public async Task<ActionResult<EntityExtractionResultDto>> ExtractEntities(
        Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Extracting entities for content {Id}", id);
        var command = new ExtractEntitiesCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get extracted entities for a content item.
    /// </summary>
    [HttpGet("{id:guid}/entities")]
    public async Task<ActionResult<IReadOnlyList<ContentEntityDto>>> GetEntities(
        Guid id, CancellationToken cancellationToken)
    {
        var entities = await _entityRepository.GetByContentItemIdAsync(id, cancellationToken);

        return Ok(entities.Select(e => new ContentEntityDto(
            e.Id, e.ContentItemId, e.Name, e.EntityType,
            e.SalienceScore, e.MentionCount)));
    }

    /// <summary>
    /// Find content similar to a given item using KNN vector search.
    /// Requires entity extraction to have been run first (generates the embedding).
    /// </summary>
    // [FromQuery] k = URL query parameter. Like req.query.k in Express.
    [HttpGet("{id:guid}/similar")]
    public async Task<ActionResult<SimilaritySearchResultDto>> FindSimilar(
        Guid id,
        [FromQuery] int k = 10,
        CancellationToken cancellationToken = default)
    {
        k = Math.Clamp(k, 1, 50); // Prevent unbounded queries
        _logger.LogInformation("Finding {K} similar items for content {Id}", k, id);
        var query = new FindSimilarContentQuery(id, k);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Run Louvain community detection to cluster all content into topic groups.
    /// Requires entity extraction to have been run on content items first.
    /// </summary>
    [HttpPost("cluster")]
    public async Task<ActionResult<ClusteringResultDto>> RunClustering(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running Louvain clustering on content graph");
        var command = new RunClusteringCommand();
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all discovered content clusters.
    /// </summary>
    [HttpGet("clusters")]
    public async Task<ActionResult<IReadOnlyList<ContentClusterDto>>> GetClusters(
        CancellationToken cancellationToken)
    {
        var clusters = await _clusterRepository.GetAllWithMembersAsync(cancellationToken);

        return Ok(clusters.Select(c => new ContentClusterDto(
            c.Id, c.Label, c.CommunityId, c.ModularityScore,
            c.TopEntities, c.MemberCount)));
    }

    /// <summary>
    /// Get members of a specific cluster.
    /// </summary>
    [HttpGet("clusters/{id:guid}/members")]
    public async Task<ActionResult<IReadOnlyList<ClusterMemberDto>>> GetClusterMembers(
        Guid id, CancellationToken cancellationToken)
    {
        var members = await _clusterRepository.GetClusterMembersAsync(id, cancellationToken);

        return Ok(members.Select(m => new ClusterMemberDto(
            m.ContentItemId,
            m.ContentItem?.BotName ?? "Unknown",
            m.ContentItem?.TextContent.Length > 200
                ? m.ContentItem.TextContent[..200] + "..."
                : m.ContentItem?.TextContent ?? "",
            m.MembershipScore)));
    }
}
