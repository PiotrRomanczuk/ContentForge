using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Interfaces.Repositories;

// Extends the generic repo with content-specific queries.
// Like creating a contentService that extends a base CrudService in Express,
// adding methods like getByStatus(), getPending(), etc.
public interface IContentItemRepository : IRepository<ContentItem>
{
    Task<IReadOnlyList<ContentItem>> GetByStatusAsync(
        ContentStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentItem>> GetByBotNameAsync(
        string botName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentItem>> GetPendingApprovalAsync(
        int skip = 0, int take = 50, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentItem>> GetScheduledForPublishingAsync(
        DateTime until, CancellationToken cancellationToken = default);

    Task<int> GetCountByStatusAsync(
        ContentStatus status, CancellationToken cancellationToken = default);

    Task BulkUpdateStatusAsync(
        IEnumerable<Guid> ids, ContentStatus status, CancellationToken cancellationToken = default);
}
