using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Entities;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services.Scheduling;

// Wraps the Hangfire RecurringJob API behind the IScheduleJobManager interface.
// RecurringJob.AddOrUpdate = like node-cron.schedule() — registers a job to run on a cron pattern.
// Hangfire persists jobs in PostgreSQL, so they survive app restarts.
public class HangfireScheduleJobManager : IScheduleJobManager
{
    private readonly ILogger<HangfireScheduleJobManager> _logger;

    public HangfireScheduleJobManager(ILogger<HangfireScheduleJobManager> logger)
    {
        _logger = logger;
    }

    public void RegisterOrUpdateRecurringJob(ScheduleConfig config)
    {
        var jobId = $"schedule-{config.Id}";

        // AddOrUpdate<T> = registers a recurring job. If the jobId already exists, it updates the schedule.
        // The lambda specifies which method to call — Hangfire serializes this and executes it later.
        RecurringJob.AddOrUpdate<AutoPublishJob>(
            jobId,
            job => job.ExecuteAsync(config.Id, CancellationToken.None),
            config.CronExpression);

        _logger.LogInformation(
            "Registered recurring job '{JobId}' with cron '{Cron}'",
            jobId, config.CronExpression);
    }

    public void RemoveRecurringJob(Guid scheduleConfigId)
    {
        var jobId = $"schedule-{scheduleConfigId}";
        RecurringJob.RemoveIfExists(jobId);
        _logger.LogInformation("Removed recurring job '{JobId}'", jobId);
    }

    public IReadOnlyList<JobStatusDto> GetAllJobStatuses()
    {
        // JobStorage.Current = Hangfire's global storage instance.
        // GetConnection() opens a read connection to query job metadata.
        using var connection = JobStorage.Current.GetConnection();
        var recurringJobs = connection.GetRecurringJobs();

        return recurringJobs.Select(j => new JobStatusDto(
            j.Id,
            j.Id, // Job name = job ID for recurring jobs
            j.Cron,
            j.NextExecution?.ToString("O"),
            j.LastExecution?.ToString("O")
        )).ToList();
    }
}
