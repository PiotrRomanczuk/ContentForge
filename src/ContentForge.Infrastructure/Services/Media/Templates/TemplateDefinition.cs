using SixLabors.ImageSharp;

namespace ContentForge.Infrastructure.Services.Media.Templates;

// Template = a visual preset for rendering social media images.
// Like a theme config object: { primaryColor: '#...', fontSize: 24, ... }
public record TemplateDefinition(
    string Name,
    string Description,
    Color PrimaryColor,
    Color SecondaryColor,
    Color TextColor,
    float TitleFontSize,
    float BodyFontSize,
    float Padding);

// Static factory — returns predefined templates (like a lookup map in JS).
public static class TemplateRegistry
{
    // Blue-to-purple gradient, white text — clean educational look
    public static TemplateDefinition EnglishFacts => new(
        "english-facts",
        "Blue-to-purple gradient for language learning posts",
        Color.ParseHex("#1a237e"),   // deep blue
        Color.ParseHex("#7b1fa2"),   // purple
        Color.White,
        TitleFontSize: 48f,
        BodyFontSize: 32f,
        Padding: 80f);

    // Dark purple-to-gold — mystical horoscope vibe
    public static TemplateDefinition Horoscope => new(
        "horoscope",
        "Dark purple-to-gold gradient for horoscope posts",
        Color.ParseHex("#1a0033"),   // deep purple
        Color.ParseHex("#b8860b"),   // dark goldenrod
        Color.White,
        TitleFontSize: 52f,
        BodyFontSize: 30f,
        Padding: 80f);

    // White background, black text — universal fallback
    public static TemplateDefinition Minimal => new(
        "minimal",
        "Clean white background with dark text",
        Color.White,
        Color.ParseHex("#f5f5f5"),   // near-white
        Color.ParseHex("#212121"),   // near-black
        TitleFontSize: 44f,
        BodyFontSize: 28f,
        Padding: 100f);

    // Carousel variants reuse the same colors with adjusted sizing
    public static TemplateDefinition EnglishFactsCarousel => new(
        "english-facts-carousel",
        "Blue-to-purple gradient for carousel slides",
        EnglishFacts.PrimaryColor, EnglishFacts.SecondaryColor, EnglishFacts.TextColor,
        TitleFontSize: 44f, BodyFontSize: 28f, Padding: 60f);

    public static TemplateDefinition HoroscopeCarousel => new(
        "horoscope-carousel",
        "Dark purple-to-gold gradient for carousel slides",
        Horoscope.PrimaryColor, Horoscope.SecondaryColor, Horoscope.TextColor,
        TitleFontSize: 48f, BodyFontSize: 26f, Padding: 60f);

    // Lookup by name — like templates[name] in JS
    public static TemplateDefinition? GetByName(string name) => name switch
    {
        "english-facts" => EnglishFacts,
        "horoscope" => Horoscope,
        "minimal" => Minimal,
        "english-facts-carousel" => EnglishFactsCarousel,
        "horoscope-carousel" => HoroscopeCarousel,
        _ => null
    };

    public static IReadOnlyList<TemplateDefinition> GetAll() => new[]
    {
        EnglishFacts, Horoscope, Minimal, EnglishFactsCarousel, HoroscopeCarousel
    };
}
