using System.Diagnostics;
using ContentForge.Application.Commands.PublishContent;
using ContentForge.Application.Commands.RenderContent;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services.Scheduling;

// Hangfire background job — picks the next Queued content item and publishes it.
// Runs on the schedule defined in ScheduleConfig.CronExpression.
// Like a BullMQ worker processor in Node.js.
public class AutoPublishJob
{
    private readonly IContentItemRepository _contentRepository;
    private readonly IScheduleConfigRepository _scheduleRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<AutoPublishJob> _logger;

    public AutoPublishJob(
        IContentItemRepository contentRepository,
        IScheduleConfigRepository scheduleRepository,
        IMediator mediator,
        ILogger<AutoPublishJob> logger)
    {
        _contentRepository = contentRepository;
        _scheduleRepository = scheduleRepository;
        _mediator = mediator;
        _logger = logger;
    }

    // Called by Hangfire when the recurring job fires.
    // The scheduleConfigId links back to the ScheduleConfig that triggered this execution.
    public async Task ExecuteAsync(Guid scheduleConfigId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AutoPublish job triggered for schedule {ScheduleId}", scheduleConfigId);
        var stopwatch = Stopwatch.StartNew();

        var config = await _scheduleRepository.GetByIdAsync(scheduleConfigId, cancellationToken);
        if (config is null || !config.IsActive)
        {
            _logger.LogWarning("Schedule {Id} not found or inactive, skipping", scheduleConfigId);
            return;
        }

        // Find the next Queued content item matching this schedule's bot and content type
        var queuedItems = await _contentRepository
            .GetByStatusAsync(ContentStatus.Queued, cancellationToken);

        var item = queuedItems.FirstOrDefault(i =>
            i.BotName == config.BotRegistration?.BotName &&
            i.ContentType == config.PreferredContentType);

        // Fallback: any queued item from the same bot
        item ??= queuedItems.FirstOrDefault(i =>
            i.BotName == config.BotRegistration?.BotName);

        if (item is null)
        {
            _logger.LogInformation(
                "No queued content for schedule {Id} (bot: {Bot}), skipping",
                scheduleConfigId, config.BotRegistration?.BotName);
            return;
        }

        // If content hasn't been rendered yet, render it first.
        // The render handler now accepts Queued status, so no status hack needed.
        if (string.IsNullOrEmpty(item.MediaPath))
        {
            try
            {
                await _mediator.Send(
                    new RenderContentCommand(item.Id), cancellationToken);

                // Reload after rendering (status changed to Rendered)
                item = await _contentRepository.GetByIdAsync(item.Id, cancellationToken);
                if (item is null) return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render content {Id} during auto-publish", item.Id);
                return;
            }
        }

        // Publish
        try
        {
            var result = await _mediator.Send(
                new PublishContentCommand(item.Id, config.SocialAccountId),
                cancellationToken);

            _logger.LogInformation(
                "Auto-publish result for content {Id}: {Success} (post: {PostId})",
                item.Id, result.IsSuccess, result.ExternalPostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-publish failed for content {Id}", item.Id);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("AutoPublish job for schedule {ScheduleId} completed in {ElapsedMs}ms",
                scheduleConfigId, stopwatch.ElapsedMilliseconds);
        }
    }
}
