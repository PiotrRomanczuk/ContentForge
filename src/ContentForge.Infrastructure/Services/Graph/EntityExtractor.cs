using ContentForge.Domain.Entities;

namespace ContentForge.Infrastructure.Services.Graph;

// Entity extraction using TF-IDF keyword analysis.
// Extracts significant terms/phrases from content text and scores them by salience (importance).
// This is a self-contained implementation — no external NLP API required.
//
// For JS devs: think of this like a keyword extraction library (e.g., `keyword-extractor` on npm),
// but with salience scoring based on term frequency vs. how "interesting" a term is.
//
// In production, you could replace this with calls to Claude, GPT, or Google NLP for better
// named entity recognition (NER). This implementation focuses on keyword-based entities.
public static class EntityExtractor
{
    // Common English stop words to filter out — like a Set of words to skip.
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with",
        "by", "from", "is", "are", "was", "were", "be", "been", "being", "have", "has", "had",
        "do", "does", "did", "will", "would", "could", "should", "may", "might", "can", "shall",
        "this", "that", "these", "those", "it", "its", "they", "them", "their", "we", "us", "our",
        "you", "your", "he", "him", "his", "she", "her", "i", "me", "my", "not", "no", "nor",
        "as", "if", "then", "else", "when", "where", "how", "what", "which", "who", "whom",
        "all", "each", "every", "both", "few", "more", "most", "other", "some", "such",
        "than", "too", "very", "just", "about", "above", "after", "again", "also", "any",
        "because", "before", "between", "during", "here", "into", "only", "over", "same",
        "so", "there", "through", "under", "until", "up", "while", "out", "down", "off"
    };

    // Simple entity type classification based on casing and patterns.
    // For real NER, use an ML model or external API.
    private static readonly Dictionary<string, string> KnownEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Common categories that appear in content management
        ["history"] = "Topic", ["science"] = "Topic", ["technology"] = "Topic",
        ["art"] = "Topic", ["music"] = "Topic", ["sports"] = "Topic",
        ["health"] = "Topic", ["education"] = "Topic", ["politics"] = "Topic",
        ["economy"] = "Topic", ["nature"] = "Topic", ["space"] = "Topic",
        ["physics"] = "Topic", ["chemistry"] = "Topic", ["biology"] = "Topic",
        ["mathematics"] = "Topic", ["philosophy"] = "Topic", ["literature"] = "Topic",
        ["astronomy"] = "Topic", ["geography"] = "Topic", ["psychology"] = "Topic",
    };

    /// <summary>
    /// Extract entities with salience scores from text content.
    /// Uses TF-based scoring: more frequent, non-stop-word terms are more salient.
    /// </summary>
    public static List<ContentEntity> Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<ContentEntity>();

        // Tokenize: split on non-alphanumeric chars, filter stop words and short words.
        var tokens = Tokenize(text);
        if (tokens.Count == 0)
            return new List<ContentEntity>();

        // Count term frequencies — like a word frequency counter.
        var termFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var originalForms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in tokens)
        {
            var lower = token.ToLowerInvariant();
            termFrequencies.TryGetValue(lower, out var count);
            termFrequencies[lower] = count + 1;

            // Keep the first-seen casing as the display form.
            if (!originalForms.ContainsKey(lower))
                originalForms[lower] = token;
        }

        // Also extract bigrams (two-word phrases) — catches compound entities like "quantum physics".
        var bigrams = ExtractBigrams(tokens);
        foreach (var (bigram, count) in bigrams)
        {
            var lower = bigram.ToLowerInvariant();
            termFrequencies[lower] = count;
            if (!originalForms.ContainsKey(lower))
                originalForms[lower] = bigram;
        }

        // Calculate max frequency for normalization.
        var maxFreq = termFrequencies.Values.Max();

        // Build entity list with salience scores (normalized TF).
        var entities = termFrequencies
            .Where(kv => kv.Value >= 1) // At least 1 occurrence
            .OrderByDescending(kv => kv.Value)
            .Take(20) // Top 20 entities per content item
            .Select(kv => new ContentEntity
            {
                Name = originalForms[kv.Key],
                NormalizedName = kv.Key,
                EntityType = ClassifyEntityType(originalForms[kv.Key]),
                SalienceScore = (double)kv.Value / maxFreq, // 0.0–1.0 normalized
                MentionCount = kv.Value
            })
            .ToList();

        return entities;
    }

    private static List<string> Tokenize(string text)
    {
        // Split on non-alphanumeric characters (keeps letters and digits together).
        // Like text.split(/[^a-zA-Z0-9]+/) in JS.
        return text
            .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '"', '\'',
                          '(', ')', '[', ']', '{', '}', '/', '\\', '-', '_', '–', '—' },
                   StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 3) // Skip very short tokens
            .Where(t => !StopWords.Contains(t))
            .Where(t => !int.TryParse(t, out _)) // Skip pure numbers
            .ToList();
    }

    private static List<(string Bigram, int Count)> ExtractBigrams(List<string> tokens)
    {
        var bigrams = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < tokens.Count - 1; i++)
        {
            // Only create bigrams from non-stop-word adjacent tokens.
            var bigram = $"{tokens[i]} {tokens[i + 1]}";
            bigrams.TryGetValue(bigram, out var count);
            bigrams[bigram] = count + 1;
        }

        // Only keep bigrams that appear more than once (meaningful phrases).
        return bigrams
            .Where(kv => kv.Value >= 2)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    private static string ClassifyEntityType(string term)
    {
        // Check known entity types first.
        if (KnownEntityTypes.TryGetValue(term, out var knownType))
            return knownType;

        // Heuristic: capitalized words are likely proper nouns (Person, Place, Organization).
        if (term.Length > 0 && char.IsUpper(term[0]) && !term.All(char.IsUpper))
            return "ProperNoun";

        // Multi-word terms are likely concepts/topics.
        if (term.Contains(' '))
            return "Concept";

        return "Keyword";
    }
}
