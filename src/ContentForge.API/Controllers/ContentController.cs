using ContentForge.Application.Commands.ApproveContent;
using ContentForge.Application.Commands.PublishContent;
using ContentForge.Application.Commands.RenderContent;
using ContentForge.Application.DTOs;
using ContentForge.Application.Validators;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentForge.API.Controllers;

// [Authorize] on the class = all endpoints require a valid JWT token.
// Like wrapping all routes in an auth middleware: router.use(requireAuth).
[ApiController]
[Route("api/content")]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly IContentItemRepository _contentRepository;
    // IMediator = MediatR's dispatcher. Like an event bus — you Send() a command,
    // it finds the matching handler and returns the result. Keeps controllers thin.
    private readonly IMediator _mediator;
    private readonly ILogger<ContentController> _logger;

    private readonly IValidator<ImportContentItemDto> _importItemValidator;

    public ContentController(
        IContentItemRepository contentRepository,
        IMediator mediator,
        ILogger<ContentController> logger,
        IValidator<ImportContentItemDto> importItemValidator)
    {
        _contentRepository = contentRepository;
        _mediator = mediator;
        _logger = logger;
        _importItemValidator = importItemValidator;
    }

    /// <summary>
    /// Import content generated externally (e.g. from Claude Code).
    /// Accepts single items or batches.
    /// </summary>
    // async Task<ActionResult<T>> = like async (req, res): Promise<Response<T>> in Express.
    // ActionResult<T> = can return Ok(data) or NotFound() or BadRequest() — typed response.
    // CancellationToken = auto-provided by ASP.NET, cancelled if the client disconnects.
    [HttpPost("import")]
    public async Task<ActionResult<ContentBatchResultDto>> Import(
        [FromBody] ImportContentRequest request,
        CancellationToken cancellationToken)
    {
        var items = new List<ContentItemDto>();
        var errors = new List<string>();

        foreach (var item in request.Items)
        {
            // Validate each item via FluentValidation instead of relying on try/catch.
            var validation = await _importItemValidator.ValidateAsync(item, cancellationToken);
            if (!validation.IsValid)
            {
                var messages = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                errors.Add($"[{item.BotName}] {messages}");
                continue;
            }

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
                errors.Add($"[{item.BotName}] {ex.Message}");
                _logger.LogError(ex, "Failed to import content item: {BotName}", item.BotName);
            }
        }

        _logger.LogInformation("Imported {Count} items ({Failed} failed)", items.Count, errors.Count);

        return Ok(new ContentBatchResultDto(request.Items.Count, items.Count, errors.Count, items, errors));
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
        // M-3: Clamp pagination to prevent DoS via unbounded queries
        take = Math.Clamp(take, 1, 200);
        _logger.LogDebug("Retrieving pending content (skip: {Skip}, take: {Take})", skip, take);
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
        _logger.LogInformation("Processing bulk approval for {Count} decisions", request.Decisions.Count);
        var command = new BulkApproveCommand(request.Decisions);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get content by status.
    /// </summary>
    // [FromQuery] = reads from URL query params. Like req.query.status in Express.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContentItemDto>>> GetByStatus(
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        // Enum.TryParse = safe parse that returns true/false instead of throwing.
        // `out var parsed` = if successful, the parsed enum value is assigned to `parsed`.
        // Like: const parsed = tryParseEnum(status); if (parsed) { ... }
        _logger.LogDebug("Retrieving content by status: {Status}", status ?? "all");
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
    // {id:guid} = route parameter with type constraint. Like /content/:id but only matches UUIDs.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContentItemDto>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var item = await _contentRepository.GetByIdAsync(id, cancellationToken);
        // `is null` = null check (like === null in JS). `is` is pattern matching syntax.
        if (item is null)
        {
            _logger.LogDebug("Content item {Id} not found", id);
            return NotFound();
        }

        return Ok(new ContentItemDto(
            item.Id, item.BotName, item.Category, item.ContentType,
            item.Status, item.TextContent, item.MediaPath,
            item.ScheduledAt, item.PublishedAt, item.CreatedAt));
    }

    /// <summary>
    /// Publish a content item to a social media platform.
    /// Moves status from Queued/Rendered → Publishing → Published/Failed.
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<PublishContentResultDto>> PublishContent(
        Guid id,
        [FromBody] PublishRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing content {Id} to account {AccountId}", id, request.SocialAccountId);
        var command = new PublishContentCommand(id, request.SocialAccountId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Render a content item into a social media image using a template.
    /// Moves status from Generated → Rendered.
    /// </summary>
    [HttpPost("{id:guid}/render")]
    public async Task<ActionResult<RenderContentResultDto>> RenderContent(
        Guid id,
        [FromBody] RenderRequestDto? request = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rendering content {Id} (template: {Template})", id, request?.TemplateName ?? "default");
        var command = new RenderContentCommand(id, request?.TemplateName, request?.Parameters);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// List available rendering templates, optionally filtered by content type.
    /// </summary>
    [HttpGet("templates")]
    public ActionResult<IReadOnlyList<AvailableTemplateDto>> GetTemplates(
        [FromQuery] string? contentType,
        // IMediaRenderer injected per-request via [FromServices] — like req.app.get('renderer') in Express.
        [FromServices] IMediaRenderer mediaRenderer)
    {
        _logger.LogDebug("Retrieving templates (contentType filter: {ContentType})", contentType ?? "all");
        var allTemplates = new List<AvailableTemplateDto>();

        // If content type specified, return templates for that type only
        if (contentType != null && Enum.TryParse<ContentType>(contentType, true, out var ct))
        {
            var names = mediaRenderer.GetAvailableTemplates(ct);
            allTemplates.AddRange(names.Select(n => new AvailableTemplateDto(
                n, $"Template for {ct}", new[] { ct.ToString() })));
        }
        else
        {
            // Return all templates grouped by their supported types
            foreach (var type in new[] { ContentType.Image, ContentType.Carousel })
            {
                var names = mediaRenderer.GetAvailableTemplates(type);
                allTemplates.AddRange(names.Select(n => new AvailableTemplateDto(
                    n, $"Template for {type}", new[] { type.ToString() })));
            }
        }

        // Deduplicate by name (e.g., "minimal" appears for both Image and Carousel)
        return Ok(allTemplates.DistinctBy(t => t.Name).ToList());
    }

    /// <summary>
    /// Dashboard stats — content counts by status.
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving content stats dashboard");
        var stats = new Dictionary<string, int>();
        // Enum.GetValues<T>() = gets all enum members as an array.
        // Like Object.values(ContentStatus) in JS.
        foreach (var status in Enum.GetValues<ContentStatus>())
        {
            stats[status.ToString()] = await _contentRepository
                .GetCountByStatusAsync(status, cancellationToken);
        }
        return Ok(stats);
    }
}

// Request DTOs — ImportContentRequest and ImportContentItemDto moved to Application/DTOs/ContentDtos.cs
// so FluentValidation validators in the Application layer can reference them.
public record BulkApproveRequest(IReadOnlyList<ApprovalDecisionDto> Decisions);
