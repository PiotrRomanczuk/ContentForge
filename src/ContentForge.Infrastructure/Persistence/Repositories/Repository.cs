using ContentForge.Domain.Common;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContentForge.Infrastructure.Persistence.Repositories;

// Generic base repository — provides CRUD for any entity type.
// Like a base service class in NestJS: `class CrudService<T>` with findOne, findAll, create, etc.
// `protected` = accessible by this class and subclasses (like `protected` in TS classes).
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ContentForgeDbContext Context;
    protected readonly DbSet<T> DbSet;
    // ILoggerFactory.CreateLogger() = creates a logger with a dynamic category name.
    // This gives us "ContentForge.Repository<ContentItem>" instead of a generic name.
    protected readonly ILogger Logger;

    // ILoggerFactory is optional — uses NullLoggerFactory when not provided (e.g., in tests).
    public Repository(ContentForgeDbContext context, ILoggerFactory? loggerFactory = null)
    {
        Context = context;
        // context.Set<T>() = gets the DbSet for type T. Like prisma[modelName] dynamically.
        DbSet = context.Set<T>();
        Logger = (loggerFactory ?? NullLoggerFactory.Instance)
            .CreateLogger($"ContentForge.Repository<{typeof(T).Name}>");
    }

    // FirstOrDefaultAsync = like .find() in JS arrays but against the DB.
    // Returns null if not found (that's what the ? in Task<T?> means).
    // `e => e.Id == id` = lambda expression, same as `(e) => e.id === id` in JS.
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("GetById({Id})", id);
        var entity = await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            Logger.LogDebug("{EntityType} {Id} not found", typeof(T).Name, id);
        return entity;
    }

    // ToListAsync() = executes the query and returns results. Like `await prisma.foo.findMany()`.
    // Until you call ToList/First/Count, EF just builds the SQL query (lazy evaluation).
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = await DbSet.ToListAsync(cancellationToken);
        Logger.LogDebug("GetAll() → {Count} results", results.Count);
        return results;
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        // SaveChangesAsync = flushes all pending changes to DB in one transaction.
        // Like calling `await prisma.$transaction(...)`. Nothing hits the DB until this call.
        await Context.SaveChangesAsync(cancellationToken);
        Logger.LogDebug("Add({Id})", entity.Id);
        return entity;
    }

    public async Task<IReadOnlyList<T>> AddRangeAsync(
        IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var list = entities.ToList();
        await DbSet.AddRangeAsync(list, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        Logger.LogDebug("AddRange({Count} entities)", list.Count);
        return list;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        // EF tracks changes on entities it loaded. Update() tells it to track this entity
        // as modified even if EF didn't load it — forces an UPDATE SQL statement.
        DbSet.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        Logger.LogDebug("Update({Id})", entity.Id);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
        Logger.LogDebug("Delete({Id})", entity.Id);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = await DbSet.CountAsync(cancellationToken);
        Logger.LogDebug("Count() → {Count}", count);
        return count;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Context.SaveChangesAsync(cancellationToken);
    }
}
