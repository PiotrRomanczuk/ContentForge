using ContentForge.Domain.Entities;
using ContentForge.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Persistence.Repositories;

// Repository for content embeddings — wraps pgvector KNN queries.
// pgvector adds a `vector` column type to PostgreSQL and supports fast similarity search.
// When pgvector isn't available, falls back to in-memory cosine distance calculation.
public class ContentEmbeddingRepository : Repository<ContentEmbedding>, IContentEmbeddingRepository
{
    public ContentEmbeddingRepository(ContentForgeDbContext context, ILoggerFactory? loggerFactory = null)
        : base(context, loggerFactory) { }

    public async Task<ContentEmbedding?> GetByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(e => e.ContentItemId == contentItemId, cancellationToken);
    }

    // KNN search using cosine distance.
    // With pgvector, this would use: ORDER BY embedding <=> $queryVector LIMIT $k
    // Fallback: loads all embeddings and computes cosine distance in memory.
    // Fine for < 100K items; for larger scale, use pgvector's HNSW or IVFFlat indexes.
    public async Task<IReadOnlyList<(ContentEmbedding Embedding, double Distance)>> FindNearestAsync(
        float[] queryVector, int k = 10, CancellationToken cancellationToken = default)
    {
        // Load all embeddings — in production with pgvector, this would be a single SQL query:
        // SELECT *, embedding <=> @query AS distance FROM content_embeddings ORDER BY distance LIMIT @k
        var all = await DbSet.ToListAsync(cancellationToken);

        return all
            .Select(e => (Embedding: e, Distance: CosineDistance(queryVector, e.Vector)))
            .OrderBy(x => x.Distance)
            .Take(k)
            .ToList();
    }

    public async Task<bool> ExistsForContentItemAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.ContentItemId == contentItemId, cancellationToken);
    }

    // Cosine distance = 1 - cosine_similarity. Range: 0 (identical) to 2 (opposite).
    // Like measuring the angle between two vectors in high-dimensional space.
    // cos(θ) = (A·B) / (|A| × |B|)
    private static double CosineDistance(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0) return 2.0;

        double dotProduct = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        var denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        if (denominator == 0) return 2.0;

        return 1.0 - (dotProduct / denominator);
    }
}
