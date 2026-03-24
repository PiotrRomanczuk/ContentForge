using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;

namespace ContentForge.Domain.Interfaces.Services;

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
