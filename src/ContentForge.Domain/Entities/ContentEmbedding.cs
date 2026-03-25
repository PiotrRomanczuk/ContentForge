using ContentForge.Domain.Common;

namespace ContentForge.Domain.Entities;

// Vector embedding for a content item — enables KNN (k-nearest neighbor) similarity search.
// Think of it as converting text into a fixed-length array of numbers that captures meaning.
// Two pieces of content with similar embeddings are semantically related.
// Stored in PostgreSQL using pgvector extension for efficient vector similarity search.
public class ContentEmbedding : BaseEntity
{
    // FK to the content item this embedding represents. One embedding per content item.
    public Guid ContentItemId { get; set; }

    // The embedding vector — a float array (e.g., 384 dimensions for MiniLM).
    // Stored as PostgreSQL `vector` type via pgvector for fast KNN queries.
    // Like a fingerprint of the content's meaning in high-dimensional space.
    public float[] Vector { get; set; } = Array.Empty<float>();

    // Which model/method generated this embedding (e.g., "tfidf", "miniml", "openai-ada").
    // Useful for versioning — if you change models, old embeddings can be re-generated.
    public string ModelName { get; set; } = string.Empty;

    // Dimension count — redundant with Vector.Length but useful for DB queries/validation.
    public int Dimensions { get; set; }

    // Navigation property back to the content item.
    public ContentItem ContentItem { get; set; } = null!;
}
