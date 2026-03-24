using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Services;

namespace ContentForge.Bots.Implementations;

public class HoroscopeBot : IBotDefinition
{
    public static readonly string[] ZodiacSigns =
    {
        "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo",
        "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"
    };

    public string Name => "HoroscopeBot";
    public string Category => "Entertainment";
    public string Description => "Daily horoscope content for all zodiac signs.";
    public IReadOnlyList<ContentType> SupportedContentTypes => new[]
    {
        ContentType.Image,
        ContentType.Carousel
    };
    public IReadOnlyList<string> SupportedLanguages => new[] { "pl", "en" };

    public string GetPromptTemplate(ContentType contentType, string language) => (contentType, language) switch
    {
        (ContentType.Carousel, "pl") => """
            Stwórz karuzelowy horoskop z 4 slajdami:
            Slajd 1: Nazwa znaku + dzisiejszy vibe
            Slajd 2: Miłość i relacje
            Slajd 3: Kariera i pieniądze
            Slajd 4: Szczęśliwa liczba, kolor i motywacyjne zakończenie
            Format JSON: [{"slide": 1, "heading": "...", "body": "..."}]
            """,
        (ContentType.Carousel, _) => """
            Create a carousel horoscope with 4 slides:
            Slide 1: Sign name + today's vibe
            Slide 2: Love & relationships
            Slide 3: Career & money
            Slide 4: Lucky number, color, motivational closing
            Format as JSON: [{"slide": 1, "heading": "...", "body": "..."}]
            """,
        (_, "pl") => """
            Napisz dzienny horoskop jako post na social media.
            Zawrzyj: emoji znaku, dzisiejszy nastrój jednym słowem,
            prognozę w 2-3 zdaniach i szczęśliwą liczbę. Max 150 słów.
            """,
        _ => """
            Write a daily horoscope as a social media post.
            Include: emoji for the sign, today's mood in one word,
            a 2-3 sentence forecast, and a lucky number. Under 150 words.
            """
    };
}
