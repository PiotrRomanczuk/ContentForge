using ContentForge.Application.DTOs;
using ContentForge.Domain.Entities;

namespace ContentForge.Application.Services;

// Abstraction over the job scheduler (Hangfire).
// Keeps Application layer decoupled from Hangfire — like an interface over BullMQ in Node.js.
public interface IScheduleJobManager
{
    void RegisterOrUpdateRecurringJob(ScheduleConfig config);
    void RemoveRecurringJob(Guid scheduleConfigId);
    IReadOnlyList<JobStatusDto> GetAllJobStatuses();
}
