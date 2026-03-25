using ContentForge.Application.Services;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services.Scheduling;

// Recurring Hangfire job — fetches engagement metrics for recently published content.
// Runs every 6 hours. Like a scheduled analytics sync in a cron worker.
public class MetricsCollectionJob
{
    private readonly IPublishRecordRepository _publishRecordRepository;
    private readonly IRepository<ContentMetric> _metricRepository;
    private readonly IPlatformAdapterFactory _adapterFactory;
    private readonly ILogger<MetricsCollectionJob> _logger;

    public MetricsCollectionJob(
        IPublishRecordRepository publishRecordRepository,
        IRepository<ContentMetric> metricRepository,
        IPlatformAdapterFactory adapterFactory,
        ILogger<MetricsCollectionJob> logger)
    {
        _publishRecordRepository = publishRecordRepository;
        _metricRepository = metricRepository;
        _adapterFactory = adapterFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Fetch metrics for posts published in the last 7 days
        var since = DateTime.UtcNow.AddDays(-7);
        _logger.LogInformation("Metrics collection job started (since: {Since})", since);
        var recentRecords = await _publishRecordRepository
            .GetRecentlyPublishedAsync(since, cancellationToken);

        var collected = 0;

        foreach (var record in recentRecords)
        {
            if (string.IsNullOrEmpty(record.ExternalPostId)) continue;

            try
            {
                _logger.LogDebug("Collecting metrics for post {PostId} on {Platform}",
                    record.ExternalPostId, record.Platform);
                var adapter = _adapterFactory.GetAdapter(record.Platform);
                var metric = await adapter.FetchMetricsAsync(
                    record.ExternalPostId, record.SocialAccount, cancellationToken);

                if (metric is null) continue;

                // Link metric to the content item
                metric.ContentItemId = record.ContentItemId;
                await _metricRepository.AddAsync(metric, cancellationToken);
                collected++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to collect metrics for post {PostId} on {Platform}",
                    record.ExternalPostId, record.Platform);
            }
        }

        _logger.LogInformation(
            "Metrics collection completed: {Collected}/{Total} posts",
            collected, recentRecords.Count);
    }
}
