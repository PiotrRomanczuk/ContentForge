namespace ContentForge.Infrastructure.Services.Publishing;

public class FacebookOptions
{
    public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com/v21.0";
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}
