using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Interfaces.Services;

/// <summary>
/// Defines a content bot category — its supported types, languages, and prompt templates.
/// Content is generated externally (e.g. via Claude Code) and imported through the API.
/// Prompt templates help the operator generate content with consistent quality.
/// </summary>
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
