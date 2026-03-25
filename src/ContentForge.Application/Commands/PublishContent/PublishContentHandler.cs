using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.PublishContent;

public class PublishContentHandler : IRequestHandler<PublishContentCommand, PublishContentResultDto>
{
    private readonly IContentItemRepository _contentRepository;
    private readonly ISocialAccountRepository _accountRepository;
    private readonly IPublishRecordRepository _publishRecordRepository;
    private readonly IPlatformAdapterFactory _adapterFactory;
    private readonly ILogger<PublishContentHandler> _logger;

    // Max retries before permanently marking as Failed
    private const int MaxRetries = 3;

    public PublishContentHandler(
        IContentItemRepository contentRepository,
        ISocialAccountRepository accountRepository,
        IPublishRecordRepository publishRecordRepository,
        IPlatformAdapterFactory adapterFactory,
        ILogger<PublishContentHandler> logger)
    {
        _contentRepository = contentRepository;
        _accountRepository = accountRepository;
        _publishRecordRepository = publishRecordRepository;
        _adapterFactory = adapterFactory;
        _logger = logger;
    }

    public async Task<PublishContentResultDto> Handle(
        PublishContentCommand request, CancellationToken cancellationToken)
    {
        var item = await _contentRepository.GetByIdAsync(request.ContentItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Content item {request.ContentItemId} not found");

        // Guard: only Queued or Rendered content can be published
        if (item.Status is not (ContentStatus.Queued or ContentStatus.Rendered))
            throw new InvalidOperationException(
                $"Content must be Queued or Rendered to publish. Current: {item.Status}");

        var account = await _accountRepository.GetByIdAsync(request.SocialAccountId, cancellationToken)
            ?? throw new InvalidOperationException($"Social account {request.SocialAccountId} not found");

        if (!account.IsActive)
            throw new InvalidOperationException($"Social account '{account.Name}' is not active");

        // Set status to Publishing immediately — acts as an optimistic lock
        // to prevent another job from picking the same item.
        item.Status = ContentStatus.Publishing;
        await _contentRepository.UpdateAsync(item, cancellationToken);

        // Resolve the platform-specific adapter (Facebook, Instagram, etc.)
        var adapter = _adapterFactory.GetAdapter(account.Platform);

        try
        {
            // PublishAsync returns a PublishRecord entity (audit trail of the attempt)
            var record = await adapter.PublishAsync(item, account, cancellationToken);

            // Persist the publish record (audit trail) — like saving a transaction log
            await _publishRecordRepository.AddAsync(record, cancellationToken);

            if (record.IsSuccess)
            {
                item.Status = ContentStatus.Published;
                item.PublishedAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "Published content {Id} to {Platform} (post: {PostId})",
                    item.Id, account.Platform, record.ExternalPostId);
            }
            else
            {
                HandleFailure(item, record.ErrorMessage);
            }

            await _contentRepository.UpdateAsync(item, cancellationToken);

            return new PublishContentResultDto(
                item.Id, record.IsSuccess, record.ExternalPostId,
                record.ErrorMessage, record.AttemptedAt);
        }
        catch (Exception ex)
        {
            HandleFailure(item, ex.Message);
            await _contentRepository.UpdateAsync(item, cancellationToken);

            _logger.LogError(ex, "Failed to publish content {Id}", item.Id);

            return new PublishContentResultDto(
                item.Id, false, null, ex.Message, DateTime.UtcNow);
        }
    }

    private void HandleFailure(Domain.Entities.ContentItem item, string? errorMessage)
    {
        item.RetryCount++;
        item.LastError = errorMessage;

        if (item.RetryCount >= MaxRetries)
        {
            item.Status = ContentStatus.Failed;
            _logger.LogWarning(
                "Content {Id} permanently failed after {Retries} retries: {Error}",
                item.Id, item.RetryCount, errorMessage);
        }
        else
        {
            // Back to Queued for retry on next job run
            item.Status = ContentStatus.Queued;
            _logger.LogWarning(
                "Content {Id} failed (attempt {Retry}/{Max}): {Error}",
                item.Id, item.RetryCount, MaxRetries, errorMessage);
        }
    }
}
