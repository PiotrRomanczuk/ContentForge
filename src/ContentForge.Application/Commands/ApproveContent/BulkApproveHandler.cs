using ContentForge.Application.DTOs;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.ApproveContent;

public class BulkApproveHandler : IRequestHandler<BulkApproveCommand, BulkApprovalResultDto>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly ILogger<BulkApproveHandler> _logger;

    public BulkApproveHandler(
        IContentItemRepository contentRepository,
        ILogger<BulkApproveHandler> logger)
    {
        _contentRepository = contentRepository;
        _logger = logger;
    }

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
