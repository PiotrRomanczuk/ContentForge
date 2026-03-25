using ContentForge.Application.Commands.ApproveContent;
using ContentForge.Application.DTOs;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContentForge.Tests.Unit.Commands;

public class BulkApproveHandlerTests
{
    private readonly Mock<IContentItemRepository> _contentRepoMock;
    private readonly Mock<ILogger<BulkApproveHandler>> _loggerMock;
    private readonly BulkApproveHandler _handler;

    public BulkApproveHandlerTests()
    {
        _contentRepoMock = new Mock<IContentItemRepository>();
        _loggerMock = new Mock<ILogger<BulkApproveHandler>>();

        _handler = new BulkApproveHandler(
            _contentRepoMock.Object,
            _loggerMock.Object);
    }

    private static ContentItem CreateTestContentItem(
        ContentStatus status = ContentStatus.Generated,
        string text = "Original text")
    {
        return new ContentItem
        {
            Id = Guid.NewGuid(),
            BotName = "EnglishFactsBot",
            Category = "Test",
            ContentType = ContentType.Image,
            Status = status,
            TextContent = text,
            Properties = new Dictionary<string, string>()
        };
    }

    [Fact]
    public async Task Handle_ApprovedItems_MovesToQueued()
    {
        var item = CreateTestContentItem();
        var decisions = new List<ApprovalDecisionDto>
        {
            new(item.Id, Approved: true)
        };

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _handler.Handle(new BulkApproveCommand(decisions), CancellationToken.None);

        result.Approved.Should().Be(1);
        result.Rejected.Should().Be(0);
        result.Edited.Should().Be(0);
        item.Status.Should().Be(ContentStatus.Queued);
        _contentRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RejectedItems_MovesToDraft()
    {
        var item = CreateTestContentItem();
        var decisions = new List<ApprovalDecisionDto>
        {
            new(item.Id, Approved: false)
        };

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _handler.Handle(new BulkApproveCommand(decisions), CancellationToken.None);

        result.Approved.Should().Be(0);
        result.Rejected.Should().Be(1);
        item.Status.Should().Be(ContentStatus.Draft);
    }

    [Fact]
    public async Task Handle_ApprovedWithEditedText_UpdatesTextAndCountsAsEdited()
    {
        var item = CreateTestContentItem(text: "Original");
        var decisions = new List<ApprovalDecisionDto>
        {
            new(item.Id, Approved: true, EditedText: "Updated text")
        };

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _handler.Handle(new BulkApproveCommand(decisions), CancellationToken.None);

        result.Approved.Should().Be(1);
        result.Edited.Should().Be(1);
        item.TextContent.Should().Be("Updated text");
        item.Status.Should().Be(ContentStatus.Queued);
    }

    [Fact]
    public async Task Handle_ApprovedWithReschedule_UpdatesScheduledAt()
    {
        var item = CreateTestContentItem();
        var rescheduleDate = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
        var decisions = new List<ApprovalDecisionDto>
        {
            new(item.Id, Approved: true, RescheduleAt: rescheduleDate)
        };

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _handler.Handle(new BulkApproveCommand(decisions), CancellationToken.None);

        result.Approved.Should().Be(1);
        item.ScheduledAt.Should().Be(rescheduleDate);
    }

    [Fact]
    public async Task Handle_MissingContentItem_SkipsAndContinues()
    {
        var existingItem = CreateTestContentItem();
        var missingId = Guid.NewGuid();
        var decisions = new List<ApprovalDecisionDto>
        {
            new(missingId, Approved: true),
            new(existingItem.Id, Approved: true)
        };

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);
        _contentRepoMock
            .Setup(r => r.GetByIdAsync(existingItem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        var result = await _handler.Handle(new BulkApproveCommand(decisions), CancellationToken.None);

        // Missing item skipped, existing item approved
        result.Approved.Should().Be(1);
        existingItem.Status.Should().Be(ContentStatus.Queued);
    }

    [Fact]
    public async Task Handle_MixedDecisions_CountsCorrectly()
    {
        var approve1 = CreateTestContentItem();
        var approve2 = CreateTestContentItem();
        var reject1 = CreateTestContentItem();
        var editItem = CreateTestContentItem(text: "Old");

        var decisions = new List<ApprovalDecisionDto>
        {
            new(approve1.Id, Approved: true),
            new(approve2.Id, Approved: true),
            new(reject1.Id, Approved: false),
            new(editItem.Id, Approved: true, EditedText: "New text")
        };

        _contentRepoMock.Setup(r => r.GetByIdAsync(approve1.Id, It.IsAny<CancellationToken>())).ReturnsAsync(approve1);
        _contentRepoMock.Setup(r => r.GetByIdAsync(approve2.Id, It.IsAny<CancellationToken>())).ReturnsAsync(approve2);
        _contentRepoMock.Setup(r => r.GetByIdAsync(reject1.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reject1);
        _contentRepoMock.Setup(r => r.GetByIdAsync(editItem.Id, It.IsAny<CancellationToken>())).ReturnsAsync(editItem);

        var result = await _handler.Handle(new BulkApproveCommand(decisions), CancellationToken.None);

        result.Approved.Should().Be(3); // 2 plain approvals + 1 edited approval
        result.Rejected.Should().Be(1);
        result.Edited.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyDecisions_ReturnsZeroCounts()
    {
        var decisions = new List<ApprovalDecisionDto>();

        var result = await _handler.Handle(new BulkApproveCommand(decisions), CancellationToken.None);

        result.Approved.Should().Be(0);
        result.Rejected.Should().Be(0);
        result.Edited.Should().Be(0);
        // No SaveChanges call when nothing to update
        _contentRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SetsUpdatedAtOnEachItem()
    {
        var item = CreateTestContentItem();
        var beforeHandle = DateTime.UtcNow;
        var decisions = new List<ApprovalDecisionDto>
        {
            new(item.Id, Approved: true)
        };

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        await _handler.Handle(new BulkApproveCommand(decisions), CancellationToken.None);

        item.UpdatedAt.Should().BeOnOrAfter(beforeHandle);
    }
}
