using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Services;

namespace ContentForge.Bots.Implementations;

// A concrete bot definition — implements IBotDefinition with specific prompt templates.
// `=>` on properties = expression-bodied members. Like: get name() { return "EnglishFactsBot"; }
public class EnglishFactsBot : IBotDefinition
{
    public string Name => "EnglishFactsBot";
    public string Category => "Language Learning";
    public string Description => "Fun and educational facts about the English language.";
    public IReadOnlyList<ContentType> SupportedContentTypes => new[]
    {
        ContentType.Image,
        ContentType.Carousel
    };
    public IReadOnlyList<string> SupportedLanguages => new[] { "pl", "en" };

    // Switch expression with tuple pattern matching — C# equivalent of:
    // if (contentType === 'Carousel' && language === 'pl') return '...'
    // else if (contentType === 'Carousel') return '...'  // _ = wildcard, matches anything
    // The `"""..."""` syntax = raw string literal (like template literals in JS but multi-line).
    public string GetPromptTemplate(ContentType contentType, string language) => (contentType, language) switch
    {
        (ContentType.Carousel, "pl") => """
            Wygeneruj karuzelowy post z 5 slajdami o ciekawostce dotyczącej języka angielskiego.
            Format JSON: [{"slide": 1, "heading": "...", "body": "..."}]
            Napisz treść po polsku z angielskimi przykładami.
            """,
        (ContentType.Carousel, _) => """
            Generate a carousel post with 5 slides about an interesting English language fact.
            Format as JSON array: [{"slide": 1, "heading": "...", "body": "..."}]
            Each slide should build on the previous one. Last slide = call to action.
            """,
        (_, "pl") => """
            Wygeneruj post o zaskakującej ciekawostce dotyczącej języka angielskiego.
            Zawrzyj: hook, ciekawostkę z przykładem, i pytanie zwiększające engagement.
            Max 200 słów. Po polsku z angielskimi przykładami.
            """,
        _ => """
            Generate a social media post about a surprising English language fact.
            Include: a hook, the fact with an example, and a question to boost engagement.
            Keep it under 200 words.
            """
    };
}
