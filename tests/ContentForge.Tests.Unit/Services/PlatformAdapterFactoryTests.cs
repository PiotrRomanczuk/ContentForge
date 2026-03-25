using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Services;
using ContentForge.Infrastructure.Services.Publishing;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ContentForge.Tests.Unit.Services;

public class PlatformAdapterFactoryTests
{
    private static Mock<IPlatformAdapter> CreateMockAdapter(Platform platform)
    {
        var mock = new Mock<IPlatformAdapter>();
        mock.Setup(a => a.Platform).Returns(platform);
        return mock;
    }

    [Fact]
    public void GetAdapter_ReturnsCorrectAdapter_ForRegisteredPlatform()
    {
        var igAdapter = CreateMockAdapter(Platform.Instagram);
        var fbAdapter = CreateMockAdapter(Platform.Facebook);
        var factory = new PlatformAdapterFactory(
            new[] { igAdapter.Object, fbAdapter.Object },
            NullLogger<PlatformAdapterFactory>.Instance);

        var result = factory.GetAdapter(Platform.Instagram);

        result.Should().BeSameAs(igAdapter.Object);
    }

    [Fact]
    public void GetAdapter_ThrowsNotSupported_ForUnregisteredPlatform()
    {
        var igAdapter = CreateMockAdapter(Platform.Instagram);
        var factory = new PlatformAdapterFactory(new[] { igAdapter.Object },
            NullLogger<PlatformAdapterFactory>.Instance);

        var act = () => factory.GetAdapter(Platform.TikTok);

        act.Should().Throw<NotSupportedException>()
            .WithMessage("*TikTok*");
    }

    [Fact]
    public void GetSupportedPlatforms_ReturnsAllRegistered()
    {
        var igAdapter = CreateMockAdapter(Platform.Instagram);
        var fbAdapter = CreateMockAdapter(Platform.Facebook);
        var ytAdapter = CreateMockAdapter(Platform.YouTube);
        var factory = new PlatformAdapterFactory(
            new[] { igAdapter.Object, fbAdapter.Object, ytAdapter.Object },
            NullLogger<PlatformAdapterFactory>.Instance);

        var result = factory.GetSupportedPlatforms();

        result.Should().HaveCount(3);
        result.Should().Contain(Platform.Instagram);
        result.Should().Contain(Platform.Facebook);
        result.Should().Contain(Platform.YouTube);
    }

    [Fact]
    public void GetSupportedPlatforms_ReturnsEmpty_WhenNoAdaptersRegistered()
    {
        var factory = new PlatformAdapterFactory(
            Enumerable.Empty<IPlatformAdapter>(),
            NullLogger<PlatformAdapterFactory>.Instance);

        var result = factory.GetSupportedPlatforms();

        result.Should().BeEmpty();
    }
}
