using ContentForge.Application.Services;
using ContentForge.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services.Scheduling;

// Runs at app startup — loads all active schedules from DB and registers them as Hangfire jobs.
// Like loading cron entries from a database on server boot in Node.js.
public static class ScheduleBootstrapper
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContentForgeDbContext>();
        var jobManager = scope.ServiceProvider.GetRequiredService<IScheduleJobManager>();
        // Use ILoggerFactory to create a logger for this static class.
        // Static classes can't use ILogger<T> injection, so we create one manually.
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("ContentForge.ScheduleBootstrapper");

        logger.LogInformation("Schedule bootstrapper starting — loading active schedules from DB");

        var activeSchedules = await dbContext.ScheduleConfigs
            .Include(s => s.BotRegistration)
            .Include(s => s.SocialAccount)
            .Where(s => s.IsActive)
            .ToListAsync();

        foreach (var schedule in activeSchedules)
        {
            logger.LogDebug("Registering schedule '{Bot}' → '{Account}' ({Cron})",
                schedule.BotRegistration?.BotName ?? "unknown",
                schedule.SocialAccount?.Name ?? "unknown",
                schedule.CronExpression);
            jobManager.RegisterOrUpdateRecurringJob(schedule);
        }

        // Register the fixed metrics collection job — runs every 6 hours
        RecurringJob.AddOrUpdate<MetricsCollectionJob>(
            "metrics-collection",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 */6 * * *");

        logger.LogInformation(
            "Schedule bootstrapper: registered {Count} schedules + metrics collection job",
            activeSchedules.Count);
    }
}
