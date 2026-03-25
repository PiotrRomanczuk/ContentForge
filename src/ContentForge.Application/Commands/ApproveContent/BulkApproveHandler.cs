using ContentForge.Application.DTOs;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.ApproveContent;

// IRequestHandler<TCommand, TResult> = handles a specific MediatR command.
// Like an Express route handler but decoupled from HTTP — the controller sends
// the command via _mediator.Send(), MediatR finds this handler via DI and calls Handle().
// This keeps business logic out of controllers (like moving logic from routes to services in Express).
public class BulkApproveHandler : IRequestHandler<BulkApproveCommand, BulkApprovalResultDto>
{
    // _ prefix = C# convention for private fields (like #private in JS classes).
    // readonly = can only be set in constructor (like Object.freeze on the reference).
    private readonly IContentItemRepository _contentRepository;
    private readonly ILogger<BulkApproveHandler> _logger;

    // Constructor injection — the DI container auto-provides these when creating this handler.
    // Like getting dependencies from a DI container in NestJS, but built into the framework.
    public BulkApproveHandler(
        IContentItemRepository contentRepository,
        ILogger<BulkApproveHandler> logger)
    {
        _contentRepository = contentRepository;
        _logger = logger;
    }

    // This is the method MediatR calls when the command is dispatched.
    public async Task<BulkApprovalResultDto> Handle(
        BulkApproveCommand request, CancellationToken cancellationToken)
    {
        var approved = 0;
        var rejected = 0;
        var edited = 0;

        foreach (var decision in request.Decisions)
        {
            var item = await _contentRepository.GetByIdAsync(decision.ContentItemId, cancellationToken);
            if (item is null)
            {
                _logger.LogWarning("Content item {Id} not found during approval", decision.ContentItemId);
                continue;
            }

            if (!decision.Approved)
            {
                item.Status = ContentStatus.Draft;
                rejected++;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(decision.EditedText))
                {
                    item.TextContent = decision.EditedText;
                    edited++;
                }

                item.Status = ContentStatus.Queued;
                item.ScheduledAt = decision.RescheduleAt ?? item.ScheduledAt;
                approved++;
            }

            item.UpdatedAt = DateTime.UtcNow;
            await _contentRepository.UpdateAsync(item, cancellationToken);
        }

        _logger.LogInformation(
            "Bulk approval complete: {Approved} approved, {Rejected} rejected, {Edited} edited",
            approved, rejected, edited);

        return new BulkApprovalResultDto(approved, rejected, edited);
    }
}
