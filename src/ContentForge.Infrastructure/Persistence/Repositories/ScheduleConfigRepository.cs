using ContentForge.Domain.Entities;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Persistence.Repositories;

public class ScheduleConfigRepository : Repository<ScheduleConfig>, IScheduleConfigRepository
{
    public ScheduleConfigRepository(ContentForgeDbContext context, ILoggerFactory? loggerFactory = null)
        : base(context, loggerFactory) { }

    public async Task<IReadOnlyList<ScheduleConfig>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GetActive()");
        // Include() = eager-load related entities. Like Prisma's `include: { botRegistration: true }`.
        return await DbSet.Where(s => s.IsActive)
            .Include(s => s.BotRegistration)
            .Include(s => s.SocialAccount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleConfig>> GetByBotRegistrationIdAsync(
        Guid botRegistrationId, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GetByBotRegistrationId({BotRegistrationId})", botRegistrationId);
        return await DbSet.Where(s => s.BotRegistrationId == botRegistrationId)
            .Include(s => s.SocialAccount)
            .ToListAsync(cancellationToken);
    }
}
