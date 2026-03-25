using ContentForge.Application.Commands.RenderContent;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using ContentForge.Domain.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContentForge.Tests.Unit.Commands;

public class RenderContentHandlerTests
{
    private readonly Mock<IContentItemRepository> _contentRepoMock;
    private readonly Mock<IMediaRenderer> _mediaRendererMock;
    private readonly Mock<ILogger<RenderContentHandler>> _loggerMock;
    private readonly RenderContentHandler _handler;

    public RenderContentHandlerTests()
    {
        _contentRepoMock = new Mock<IContentItemRepository>();
        _mediaRendererMock = new Mock<IMediaRenderer>();
        _loggerMock = new Mock<ILogger<RenderContentHandler>>();

        _handler = new RenderContentHandler(
            _contentRepoMock.Object,
            _mediaRendererMock.Object,
            _loggerMock.Object);
    }

    private static ContentItem CreateTestContentItem(
        ContentStatus status = ContentStatus.Generated,
        ContentType contentType = ContentType.Image,
        string botName = "EnglishFactsBot",
        string text = "Test content text")
    {
        return new ContentItem
        {
            Id = Guid.NewGuid(),
            BotName = botName,
            Category = "Test",
            ContentType = contentType,
            Status = status,
            TextContent = text,
            Properties = new Dictionary<string, string>()
        };
    }

    [Fact]
    public async Task Handle_WhenContentNotFound_ThrowsInvalidOperation()
    {
        var command = new RenderContentCommand(Guid.NewGuid());
        _contentRepoMock
            .Setup(r => r.GetByIdAsync(command.ContentItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentItem?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WhenStatusNotGenerated_ThrowsInvalidOperation()
    {
        var item = CreateTestContentItem(status: ContentStatus.Draft);
        var command = new RenderContentCommand(item.Id);
        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Generated*");
    }

    [Fact]
    public async Task Handle_ImageContent_CallsRenderImageAsync()
    {
        var item = CreateTestContentItem(contentType: ContentType.Image);
        var command = new RenderContentCommand(item.Id);
        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _mediaRendererMock
            .Setup(m => m.RenderImageAsync(
                item.TextContent, "english-facts", It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/media/rendered/test.png");

        await _handler.Handle(command, CancellationToken.None);

        _mediaRendererMock.Verify(
            m => m.RenderImageAsync(
                item.TextContent, "english-facts", It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CarouselContent_CallsRenderCarouselAsync()
    {
        var item = CreateTestContentItem(
            contentType: ContentType.Carousel,
            text: "[{\"heading\":\"H1\",\"body\":\"B1\"},{\"heading\":\"H2\",\"body\":\"B2\"},{\"heading\":\"H3\",\"body\":\"B3\"}]");
        var command = new RenderContentCommand(item.Id);
        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _mediaRendererMock
            .Setup(m => m.RenderCarouselAsync(
                It.Is<IEnumerable<string>>(s => s.Count() == 3),
                "english-facts-carousel",
                It.IsAny<Dictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("/media/rendered/carousel.zip");

        await _handler.Handle(command, CancellationToken.None);

        _mediaRendererMock.Verify(
            m => m.RenderCarouselAsync(
                It.Is<IEnumerable<string>>(s => s.Count() == 3),
                "english-facts-carousel",
                It.IsAny<Dictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatesMediaPathAndStatus()
    {
        var item = CreateTestContentItem();
        var command = new RenderContentCommand(item.Id);
        var expectedPath = "/media/rendered/output.png";

        _contentRepoMock
            .Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _mediaRendererMock
            .Setup(m => m.RenderImageAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPath);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.MediaPath.Should().Be(expectedPath);
        item.MediaPath.Should().Be(expectedPath);
        item.Status.Should().Be(ContentStatus.Rendered);
        _contentRepoMock.Verify(
            r => r.UpdateAsync(item, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("EnglishFactsBot", "english-facts")]
    [InlineData("HoroscopeBot", "horoscope")]
    [InlineData("UnknownBot", "minimal")]
    public async Task Handle_InfersTemplateFromBotName(string botName, string expectedTemplate)
    {
        // Verify template inference by checking which template name is passed to the renderer
        var item = CreateTestContentItem(status: ContentStatus.Generated);
        item.BotName = botName;
        _contentRepoMock.Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _mediaRendererMock.Setup(r => r.RenderImageAsync(
                It.IsAny<string>(), expectedTemplate, It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/rendered/test.png");

        await _handler.Handle(new RenderContentCommand(item.Id), CancellationToken.None);

        _mediaRendererMock.Verify(r => r.RenderImageAsync(
            It.IsAny<string>(), expectedTemplate, It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
