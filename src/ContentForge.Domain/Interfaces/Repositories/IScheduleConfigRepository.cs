using ContentForge.Domain.Entities;

namespace ContentForge.Domain.Interfaces.Repositories;

public interface IScheduleConfigRepository : IRepository<ScheduleConfig>
{
    Task<IReadOnlyList<ScheduleConfig>> GetActiveAsync(
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduleConfig>> GetByBotRegistrationIdAsync(
        Guid botRegistrationId, CancellationToken cancellationToken = default);
}
