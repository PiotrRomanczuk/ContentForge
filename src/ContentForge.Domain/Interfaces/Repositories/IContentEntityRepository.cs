using ContentForge.Domain.Entities;

namespace ContentForge.Domain.Interfaces.Repositories;

// Repository for extracted content entities — like a specialized service for the entity graph.
// Provides queries for finding shared entities between content items (graph edges).
public interface IContentEntityRepository : IRepository<ContentEntity>
{
    // Get all entities extracted from a specific content item.
    Task<IReadOnlyList<ContentEntity>> GetByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);

    // Find content items that share entities with a given item (graph neighbors).
    // Returns entity names + combined salience scores — the edge weights for Louvain.
    Task<IReadOnlyList<ContentEntity>> GetByNormalizedNameAsync(
        string normalizedName, CancellationToken cancellationToken = default);

    // Get all distinct entity names across all content — the full node set for the graph.
    Task<IReadOnlyList<string>> GetDistinctEntityNamesAsync(
        CancellationToken cancellationToken = default);

    // Bulk insert entities for a content item (used after entity extraction).
    Task AddEntitiesForContentAsync(
        Guid contentItemId, IEnumerable<ContentEntity> entities,
        CancellationToken cancellationToken = default);
}
