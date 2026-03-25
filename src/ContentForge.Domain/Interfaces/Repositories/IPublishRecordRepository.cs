using ContentForge.Domain.Entities;

namespace ContentForge.Domain.Interfaces.Repositories;

public interface IPublishRecordRepository : IRepository<PublishRecord>
{
    Task<IReadOnlyList<PublishRecord>> GetByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublishRecord>> GetRecentlyPublishedAsync(
        DateTime since, CancellationToken cancellationToken = default);
}
