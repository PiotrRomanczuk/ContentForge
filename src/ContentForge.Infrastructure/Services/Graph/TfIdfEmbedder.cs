namespace ContentForge.Infrastructure.Services.Graph;

// TF-IDF (Term Frequency - Inverse Document Frequency) vector embedder.
// Converts text into a fixed-length float array that captures its meaning relative to a corpus.
//
// For JS devs: like a simple version of OpenAI's text-embedding-ada model, but runs locally.
// TF = how often a word appears in this document (like word.count / total.words).
// IDF = how rare a word is across all documents (rare words are more informative).
// TF-IDF = TF × IDF — high score means the word is frequent here but rare elsewhere.
//
// The output vector has one dimension per unique term in the vocabulary.
// Two documents with similar TF-IDF vectors are about similar topics.
public static class TfIdfEmbedder
{
    // Common stop words to exclude from the vocabulary.
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with",
        "by", "from", "is", "are", "was", "were", "be", "been", "being", "have", "has", "had",
        "do", "does", "did", "will", "would", "could", "should", "may", "might", "can", "shall",
        "this", "that", "these", "those", "it", "its", "they", "them", "their", "we", "us", "our",
        "you", "your", "he", "him", "his", "she", "her", "i", "me", "my", "not", "no", "as",
        "if", "then", "else", "when", "where", "how", "what", "which", "who", "all", "each",
        "every", "both", "few", "more", "most", "other", "some", "such", "than", "too", "very",
        "just", "about", "also", "any", "because", "before", "between", "here", "into", "only",
        "so", "there", "through", "under", "until", "up", "while", "out", "down", "off", "nor"
    };

    /// <summary>
    /// Generate a TF-IDF embedding vector for a document given the full corpus.
    /// Returns a float[] where each dimension = TF-IDF score for one vocabulary term.
    /// </summary>
    public static float[] GenerateEmbedding(string document, IReadOnlyList<string> corpus)
    {
        // Step 1: Build vocabulary from the entire corpus.
        var tokenizedCorpus = corpus.Select(Tokenize).ToList();
        var vocabulary = BuildVocabulary(tokenizedCorpus);

        if (vocabulary.Count == 0)
            return Array.Empty<float>();

        // Step 2: Calculate IDF for each term.
        // IDF(t) = log(N / df(t)) where N = total docs, df(t) = docs containing term t.
        var n = tokenizedCorpus.Count;
        var idf = new Dictionary<string, double>();

        foreach (var term in vocabulary)
        {
            var documentFrequency = tokenizedCorpus.Count(doc => doc.Contains(term));
            // +1 smoothing to avoid division by zero and dampen rare terms.
            idf[term] = Math.Log((double)(n + 1) / (documentFrequency + 1)) + 1;
        }

        // Step 3: Calculate TF-IDF vector for the target document.
        var docTokens = Tokenize(document);
        var termCounts = docTokens
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => g.Count());

        var maxTf = termCounts.Values.DefaultIfEmpty(1).Max();

        // Build the vector — one dimension per vocabulary term.
        var vocabList = vocabulary.ToList();
        var vector = new float[vocabList.Count];

        for (int i = 0; i < vocabList.Count; i++)
        {
            var term = vocabList[i];
            termCounts.TryGetValue(term, out var tf);
            // Normalized TF × IDF. Normalize TF to prevent long documents from dominating.
            vector[i] = (float)((double)tf / maxTf * idf.GetValueOrDefault(term, 1.0));
        }

        // L2 normalize the vector — makes cosine distance equivalent to Euclidean distance.
        // Like normalizing a JS array so its values sum-of-squares = 1.
        var norm = (float)Math.Sqrt(vector.Sum(v => v * v));
        if (norm > 0)
        {
            for (int i = 0; i < vector.Length; i++)
                vector[i] /= norm;
        }

        return vector;
    }

    private static HashSet<string> Tokenize(string text)
    {
        return text
            .ToLowerInvariant()
            .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '"', '\'',
                          '(', ')', '[', ']', '{', '}', '/', '\\', '-', '_' },
                   StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 3)
            .Where(t => !StopWords.Contains(t))
            .ToHashSet();
    }

    private static HashSet<string> BuildVocabulary(List<HashSet<string>> tokenizedCorpus)
    {
        var vocab = new HashSet<string>();
        foreach (var doc in tokenizedCorpus)
            foreach (var term in doc)
                vocab.Add(term);
        return vocab;
    }
}
