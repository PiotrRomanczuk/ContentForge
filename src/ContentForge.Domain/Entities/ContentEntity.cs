using ContentForge.Domain.Common;

namespace ContentForge.Domain.Entities;

// A named entity extracted from content text (person, place, topic, concept).
// Like a node in a knowledge graph — each entity has a name, type, and salience score.
// "Salience" = how important/central this entity is to the content (0.0 to 1.0).
public class ContentEntity : BaseEntity
{
    // FK to the content item this entity was extracted from.
    public Guid ContentItemId { get; set; }

    // The entity's display name (e.g., "Albert Einstein", "Berlin", "quantum physics").
    public string Name { get; set; } = string.Empty;

    // Normalized key for matching — lowercase, trimmed. Like a slug for deduplication.
    public string NormalizedName { get; set; } = string.Empty;

    // Entity type classification (Person, Location, Topic, Organization, Event, Concept).
    public string EntityType { get; set; } = string.Empty;

    // Salience score: 0.0–1.0, how important this entity is within the content.
    // Used as edge weight in the Louvain community detection graph.
    public double SalienceScore { get; set; }

    // Optional: count of how many times this entity appears in the content text.
    public int MentionCount { get; set; } = 1;

    // Navigation property — like a Prisma relation back to the parent content item.
    public ContentItem ContentItem { get; set; } = null!;
}
