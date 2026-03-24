using ContentForge.Application.Commands.ApproveContent;
using ContentForge.Application.DTOs;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentForge.API.Controllers;

[ApiController]
[Route("api/content")]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly IContentItemRepository _contentRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<ContentController> _logger;

    public ContentController(
        IContentItemRepository contentRepository,
        IMediator mediator,
        ILogger<ContentController> logger)
    {
        _contentRepository = contentRepository;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Import content generated externally (e.g. from Claude Code).
    /// Accepts single items or batches.
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<ContentBatchResultDto>> Import(
        [FromBody] ImportContentRequest request,
        CancellationToken cancellationToken)
    {
        var items = new List<ContentItemDto>();
        var failed = 0;

        foreach (var item in request.Items)
        {
            try
            {
                var entity = new ContentItem
                {
                    BotName = item.BotName,
                    Category = item.Category,
                    ContentType = Enum.Parse<ContentType>(item.ContentType, ignoreCase: true),
                    TextContent = item.TextContent,
                    Status = ContentStatus.Generated,
                    ScheduledAt = item.ScheduledAt,
                    Properties = item.Properties ?? new Dictionary<string, string>()
                };

                var saved = await _contentRepository.AddAsync(entity, cancellationToken);

                items.Add(new ContentItemDto(
                    saved.Id, saved.BotName, saved.Category, saved.ContentType,
                    saved.Status, saved.TextContent, saved.MediaPath,
                    saved.ScheduledAt, saved.PublishedAt, saved.CreatedAt));
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogError(ex, "Failed to import content item: {BotName}", item.BotName);
            }
        }

        _logger.LogInformation("Imported {Count} items ({Failed} failed)", items.Count, failed);

        return Ok(new ContentBatchResultDto(request.Items.Count, items.Count, failed, items));
    }

    /// <summary>
    /// Get all content pending approval (for editorial review session).
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<ContentItemDto>>> GetPending(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var items = await _contentRepository.GetPendingApprovalAsync(skip, take, cancellationToken);

        return Ok(items.Select(i => new ContentItemDto(
            i.Id, i.BotName, i.Category, i.ContentType,
            i.Status, i.TextContent, i.MediaPath,
            i.ScheduledAt, i.PublishedAt, i.CreatedAt)));
    }

    /// <summary>
    /// Bulk approve/reject content items (editorial session).
    /// </summary>
    [HttpPost("approve")]
    public async Task<ActionResult<BulkApprovalResultDto>> BulkApprove(
        [FromBody] BulkApproveRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BulkApproveCommand(request.Decisions);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get content by status.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContentItemDto>>> GetByStatus(
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var items = status != null && Enum.TryParse<ContentStatus>(status, true, out var parsed)
            ? await _contentRepository.GetByStatusAsync(parsed, cancellationToken)
            : await _contentRepository.GetAllAsync(cancellationToken);

        return Ok(items.Select(i => new ContentItemDto(
            i.Id, i.BotName, i.Category, i.ContentType,
            i.Status, i.TextContent, i.MediaPath,
            i.ScheduledAt, i.PublishedAt, i.CreatedAt)));
    }

    /// <summary>
    /// Get content item by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContentItemDto>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var item = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (item is null) return NotFound();

        return Ok(new ContentItemDto(
            item.Id, item.BotName, item.Category, item.ContentType,
            item.Status, item.TextContent, item.MediaPath,
            item.ScheduledAt, item.PublishedAt, item.CreatedAt));
    }

    /// <summary>
    /// Dashboard stats — content counts by status.
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        CancellationToken cancellationToken)
    {
        var stats = new Dictionary<string, int>();
        foreach (var status in Enum.GetValues<ContentStatus>())
        {
            stats[status.ToString()] = await _contentRepository
                .GetCountByStatusAsync(status, cancellationToken);
        }
        return Ok(stats);
    }
}

// Request DTOs
public record ImportContentRequest(IReadOnlyList<ImportContentItem> Items);

public record ImportContentItem(
    string BotName,
    string Category,
    string ContentType,
    string TextContent,
    DateTime? ScheduledAt = null,
    Dictionary<string, string>? Properties = null);

public record BulkApproveRequest(IReadOnlyList<ApprovalDecisionDto> Decisions);
