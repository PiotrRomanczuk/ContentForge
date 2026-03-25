using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Interfaces.Services;

// Not implemented yet. Each social platform (FB, IG, TikTok, YT) will get its own
// adapter class — like creating separate API client modules in Node.js.
// This is the Adapter Pattern: same interface, different implementations per platform.
public interface IPlatformAdapter
{
    Platform Platform { get; }

    Task<PublishRecord> PublishAsync(
        ContentItem content,
        SocialAccount account,
        CancellationToken cancellationToken = default);

    Task<ContentMetric?> FetchMetricsAsync(
        string externalPostId,
        SocialAccount account,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateAccountAsync(
        SocialAccount account,
        CancellationToken cancellationToken = default);
}
