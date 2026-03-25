using ContentForge.Domain.Enums;
using ContentForge.Infrastructure.Services.Media.Templates;
using FluentAssertions;

namespace ContentForge.Tests.Unit.Services;

public class TemplateRegistryTests
{
    [Theory]
    [InlineData("minimal")]
    [InlineData("english-facts")]
    [InlineData("horoscope")]
    [InlineData("english-facts-carousel")]
    [InlineData("horoscope-carousel")]
    public void GetByName_ReturnsCorrectTemplate(string name)
    {
        var result = TemplateRegistry.GetByName(name);

        result.Should().NotBeNull();
        result!.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("nonexistent")]
    [InlineData("")]
    [InlineData("English-Facts")]
    public void GetByName_ReturnsNull_ForUnknownName(string name)
    {
        var result = TemplateRegistry.GetByName(name);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_ReturnsFiveTemplates()
    {
        var result = TemplateRegistry.GetAll();

        result.Should().HaveCount(5);
        result.Select(t => t.Name).Should().Contain(new[]
        {
            "minimal", "english-facts", "horoscope",
            "english-facts-carousel", "horoscope-carousel"
        });
    }
}
