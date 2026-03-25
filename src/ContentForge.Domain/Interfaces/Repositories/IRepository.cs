namespace ContentForge.Domain.Interfaces.Repositories;

// Generic repository interface — like a base DAO/service in JS.
// `IRepository<T>` = a TypeScript generic: `interface IRepository<T>`.
// `where T : class` = constraint: T must be a reference type (no primitives like int).
//
// CancellationToken = like AbortController.signal in fetch(). Lets the caller cancel
// long-running DB operations (e.g., if the HTTP request is aborted by the client).
// `= default` means callers can omit it — it'll just be a no-op token.
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    // Flush all pending changes to the database in a single transaction.
    // Like calling prisma.$transaction() — groups multiple Add/Update/Delete calls
    // into one DB round-trip. Use this when batching multiple operations.
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
