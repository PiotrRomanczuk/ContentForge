using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Interfaces.Services;

// Interface = like a TypeScript `interface`. Defines a contract that classes must implement.
// In JS you'd just duck-type it; in C# the compiler enforces it.
//
// Each bot (EnglishFactsBot, HoroscopeBot) implements this to define what content
// it can generate and provide prompt templates for external AI tools.
public interface IBotDefinition
{
    string Name { get; }
    string Category { get; }
    string Description { get; }
    IReadOnlyList<ContentType> SupportedContentTypes { get; }
    IReadOnlyList<string> SupportedLanguages { get; }

    /// <summary>
    /// Returns a prompt template the operator can use in Claude Code or any AI tool
    /// to generate content matching this bot's format and style.
    /// </summary>
    string GetPromptTemplate(ContentType contentType, string language);
}
