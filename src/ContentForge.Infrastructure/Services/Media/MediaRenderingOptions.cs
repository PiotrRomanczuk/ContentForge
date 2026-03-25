namespace ContentForge.Infrastructure.Services.Media;

// Options pattern — like a typed config object bound from appsettings.json.
// In Node.js this would be: const config = require('./config').mediaRendering
// ASP.NET injects it as IOptions<MediaRenderingOptions> (lazy-loaded, singleton).
public class MediaRenderingOptions
{
    public string OutputDirectory { get; set; } = "./rendered-media";
    public int DefaultWidth { get; set; } = 1080;
    public int DefaultHeight { get; set; } = 1080;
    public int ThumbnailWidth { get; set; } = 400;
    public string? FontPath { get; set; }
}
