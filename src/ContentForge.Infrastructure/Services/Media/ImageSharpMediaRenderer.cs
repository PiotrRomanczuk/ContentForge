using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Services;
using ContentForge.Infrastructure.Services.Media.Templates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ContentForge.Infrastructure.Services.Media;

// Implements IMediaRenderer using SixLabors.ImageSharp — a cross-platform image library.
// Think of it as the .NET equivalent of Node.js's `sharp` or `canvas` libraries.
// Registered as Scoped (new instance per HTTP request) since it does file I/O.
public class ImageSharpMediaRenderer : IMediaRenderer
{
    private readonly MediaRenderingOptions _options;
    private readonly ILogger<ImageSharpMediaRenderer> _logger;
    private readonly FontFamily _fontFamily;

    public ImageSharpMediaRenderer(
        IOptions<MediaRenderingOptions> options,
        ILogger<ImageSharpMediaRenderer> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Load font: try custom path first, fall back to system fonts.
        // SystemFonts = like os.fonts() — reads installed fonts from the OS.
        if (!string.IsNullOrEmpty(_options.FontPath) && File.Exists(_options.FontPath))
        {
            var collection = new FontCollection();
            _fontFamily = collection.Add(_options.FontPath);
        }
        else
        {
            // Fall back to common system fonts. FontFamily is a struct (value type),
            // so we use FirstOrDefault + a name check instead of null coalescing.
            var preferred = SystemFonts.Families
                .FirstOrDefault(f => f.Name is "Arial" or "Helvetica" or "DejaVu Sans"
                    or "Liberation Sans" or "Segoe UI");
            _fontFamily = preferred.Name != null ? preferred : SystemFonts.Families.First();
        }
    }

    public async Task<string> RenderImageAsync(
        string text, string templateName,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var template = TemplateRegistry.GetByName(templateName) ?? TemplateRegistry.Minimal;
        var outputDir = EnsureOutputDirectory();
        var fileName = $"{Guid.NewGuid()}.png";
        var filePath = Path.Combine(outputDir, fileName);

        using var image = CreateImage(template, text);
        await image.SaveAsPngAsync(filePath, cancellationToken);

        _logger.LogInformation("Rendered image: {Path} ({Template})", filePath, templateName);
        return filePath;
    }

    public async Task<string> RenderCarouselAsync(
        IEnumerable<string> slides, string templateName,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var template = TemplateRegistry.GetByName(templateName) ?? TemplateRegistry.Minimal;
        var carouselId = Guid.NewGuid().ToString();
        var carouselDir = Path.Combine(EnsureOutputDirectory(), carouselId);
        Directory.CreateDirectory(carouselDir);

        var slideList = slides.ToList();
        for (var i = 0; i < slideList.Count; i++)
        {
            var slideText = $"[{i + 1}/{slideList.Count}]\n\n{slideList[i]}";
            using var image = CreateImage(template, slideText);
            var slidePath = Path.Combine(carouselDir, $"slide-{i + 1}.png");
            await image.SaveAsPngAsync(slidePath, cancellationToken);
        }

        _logger.LogInformation(
            "Rendered carousel: {Dir} ({Count} slides, {Template})",
            carouselDir, slideList.Count, templateName);

        // Return path to the first slide as the primary media path
        return Path.Combine(carouselDir, "slide-1.png");
    }

    public IReadOnlyList<string> GetAvailableTemplates(ContentType contentType)
    {
        return contentType switch
        {
            ContentType.Image or ContentType.Text =>
                new[] { "english-facts", "horoscope", "minimal" },
            ContentType.Carousel =>
                new[] { "english-facts-carousel", "horoscope-carousel", "minimal" },
            _ => new[] { "minimal" }
        };
    }

    // Creates an image with a gradient background and text overlay.
    // Image<Rgba32> = an in-memory bitmap. Like an HTML Canvas context.
    private Image<Rgba32> CreateImage(TemplateDefinition template, string text)
    {
        var width = _options.DefaultWidth;
        var height = _options.DefaultHeight;
        var image = new Image<Rgba32>(width, height);

        // Mutate = apply transformations in-place (like canvas.getContext('2d') operations).
        image.Mutate(ctx =>
        {
            // Draw gradient background
            var brush = new LinearGradientBrush(
                new PointF(0, 0),
                new PointF(width, height),
                GradientRepetitionMode.None,
                new ColorStop(0, template.PrimaryColor),
                new ColorStop(1, template.SecondaryColor));
            ctx.Fill(brush, new RectangleF(0, 0, width, height));

            // Draw text with word-wrapping within padded bounds
            var font = _fontFamily.CreateFont(template.BodyFontSize, FontStyle.Regular);
            var maxTextWidth = width - (template.Padding * 2);

            // RichTextOptions controls text layout — like CSS text properties.
            var textOptions = new RichTextOptions(font)
            {
                Origin = new PointF(template.Padding, template.Padding),
                WrappingLength = maxTextWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                LineSpacing = 1.4f
            };

            ctx.DrawText(textOptions, TruncateForImage(text), template.TextColor);
        });

        return image;
    }

    // Truncate text to avoid overflow — social media images need concise text.
    private static string TruncateForImage(string text, int maxChars = 600)
    {
        // Strip hashtags (they're for the caption, not the image)
        var lines = text.Split('\n')
            .Where(line => !line.TrimStart().StartsWith('#'))
            .ToArray();
        var cleaned = string.Join('\n', lines).Trim();

        return cleaned.Length <= maxChars
            ? cleaned
            : cleaned[..maxChars] + "...";
    }

    private string EnsureOutputDirectory()
    {
        Directory.CreateDirectory(_options.OutputDirectory);
        return _options.OutputDirectory;
    }
}
