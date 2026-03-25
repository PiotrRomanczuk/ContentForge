using ContentForge.Application.Commands.PublishContent;
using ContentForge.Application.Services;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Domain.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContentForge.Tests.Unit.Commands;

public class PublishContentHandlerTests
{
    private readonly Mock<IContentItemRepository> _contentRepoMock;
    private readonly Mock<ISocialAccountRepository> _accountRepoMock;
    private readonly Mock<IPublishRecordRepository> _publishRecordRepoMock;
    private readonly Mock<IPlatformAdapterFactory> _adapterFactoryMock;
    private readonly Mock<IPlatformAdapter> _adapterMock;
    private readonly Mock<ILogger<PublishContentHandler>> _loggerMock;
    private readonly PublishContentHandler _handler;

    public PublishContentHandlerTests()
    {
        _contentRepoMock = new Mock<IContentItemRepository>();
        _accountRepoMock = new Mock<ISocialAccountRepository>();
        _publishRecordRepoMock = new Mock<IPublishRecordRepository>();
        _adapterFactoryMock = new Mock<IPlatformAdapterFactory>();
        _adapterMock = new Mock<IPlatformAdapter>();
        _loggerMock = new Mock<ILogger<PublishContentHandler>>();

        _handler = new PublishContentHandler(
            _contentRepoMock.Object,
            _accountRepoMock.Object,
            _publishRecordRepoMock.Object,
            _adapterFactoryMock.Object,
            _loggerMock.Object);
    }

    private static ContentItem CreateTestContentItem(
        ContentStatus status = ContentStatus.Queued,
        int retryCount = 0)
    {
        return new ContentItem
        {
            Id = Guid.NewGuid(),
            BotName = "EnglishFactsBot",
            Category = "Test",
            ContentType = ContentType.Image,
            Status = status,
            TextContent = "Test content",
            RetryCount = retryCount
        };
    }

    private static SocialAccount CreateTestAccount(
        bool isActive = true,
        Platform platform = Platform.Instagram)
    {
        return new SocialAccount
        {
            Id = Guid.NewGuid(),
            Name = "Test Account",
            Platform = platform,
            ExternalId = "ext-123",
            AccessToken = "token-abc",
            IsActive = isActive
        };
    }

    [Fact]
    public async Task Handle_WhenContentNotFound_ThrowsInvalidOperation()
    {
        var command = new PublishContentCommand(Guid.NewGuid(), Guid.NewGuid());
        _contentRepoMock
            .Setup(r => r.GetByIdAsync(command.ContentItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Content item*not found*");
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ThrowsInvalidOperation()
    {
        var content = CreateTestContentItem();
        var command = new PublishContentCommand(content.Id, Guid.NewGuid());
        _contentRepoMock
            .Setup(r => r.GetByIdAsync(content.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);
        _accountRepoMock
            .Setup(r => r.GetByIdAsync(command.SocialAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SocialAccount?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Social account*not found*");
    }

    [Theory]
    [InlineData(ContentStatus.Draft)]
    [InlineData(ContentStatus.Generated)]
    [InlineData(ContentStatus.Publishing)]
    [InlineData(ContentStatus.Published)]
    [InlineData(ContentStatus.Failed)]
    public async Task Handle_WhenStatusNotQueuedOrRendered_ThrowsInvalidOperation(
        ContentStatus invalidStatus)
    {
        var content = CreateTestContentItem(status: invalidStatus);
        var account = CreateTestAccount();
        var command = new PublishContentCommand(content.Id, account.Id);

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(content.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);
        _accountRepoMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Queued or Rendered*");
    }

    [Fact]
    public async Task Handle_WhenAccountInactive_ThrowsInvalidOperation()
    {
        var content = CreateTestContentItem();
        var account = CreateTestAccount(isActive: false);
        var command = new PublishContentCommand(content.Id, account.Id);

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(content.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);
        _accountRepoMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task Handle_SuccessfulPublish_SetsPublishedStatus()
    {
        var content = CreateTestContentItem();
        var account = CreateTestAccount();
        var command = new PublishContentCommand(content.Id, account.Id);
        var expectedRecord = new PublishRecord
        {
            ContentItemId = content.Id,
            SocialAccountId = account.Id,
            Platform = account.Platform,
            IsSuccess = true,
            ExternalPostId = "post-456"
        };

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(content.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);
        _accountRepoMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _adapterFactoryMock
            .Setup(f => f.GetAdapter(account.Platform))
            .Returns(_adapterMock.Object);
        _adapterMock
            .Setup(a => a.PublishAsync(content, account, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecord);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("post-456");
        content.Status.Should().Be(ContentStatus.Published);
        content.PublishedAt.Should().NotBeNull();
        // UpdateAsync called twice: once for Publishing, once for Published.
        _contentRepoMock.Verify(
            r => r.UpdateAsync(content, It.IsAny<CancellationToken>()), Times.Exactly(2));
        // Verify publish record was persisted
        _publishRecordRepoMock.Verify(
            r => r.AddAsync(It.IsAny<PublishRecord>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FailedPublish_IncrementsRetryCount()
    {
        var content = CreateTestContentItem(retryCount: 0);
        var account = CreateTestAccount();
        var command = new PublishContentCommand(content.Id, account.Id);

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(content.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);
        _accountRepoMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _adapterFactoryMock
            .Setup(f => f.GetAdapter(account.Platform))
            .Returns(_adapterMock.Object);
        _adapterMock
            .Setup(a => a.PublishAsync(content, account, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("API error");
        content.RetryCount.Should().Be(1);
        content.LastError.Should().Be("API error");
        // Reset to Queued for retry (not yet at max).
        content.Status.Should().Be(ContentStatus.Queued);
    }

    [Fact]
    public async Task Handle_MaxRetriesExceeded_SetsFailedStatus()
    {
        // Start at MaxRetries - 1 so the next failure hits the limit.
        var content = CreateTestContentItem(
            retryCount: 3 - 1);
        var account = CreateTestAccount();
        var command = new PublishContentCommand(content.Id, account.Id);

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(content.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);
        _accountRepoMock
            .Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _adapterFactoryMock
            .Setup(f => f.GetAdapter(account.Platform))
            .Returns(_adapterMock.Object);
        _adapterMock
            .Setup(a => a.PublishAsync(content, account, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permanent failure"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        content.RetryCount.Should().Be(3);
        content.Status.Should().Be(ContentStatus.Failed);
    }
}
