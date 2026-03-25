using ContentForge.Application.Commands.ManageSchedule;
using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContentForge.Tests.Unit.Commands;

public class CreateScheduleHandlerTests
{
    private readonly Mock<IScheduleConfigRepository> _scheduleRepoMock;
    private readonly Mock<IRepository<BotRegistration>> _botRegRepoMock;
    private readonly Mock<ISocialAccountRepository> _accountRepoMock;
    private readonly Mock<IScheduleJobManager> _jobManagerMock;
    private readonly Mock<ILogger<CreateScheduleHandler>> _loggerMock;
    private readonly CreateScheduleHandler _handler;

    public CreateScheduleHandlerTests()
    {
        _scheduleRepoMock = new Mock<IScheduleConfigRepository>();
        _botRegRepoMock = new Mock<IRepository<BotRegistration>>();
        _accountRepoMock = new Mock<ISocialAccountRepository>();
        _jobManagerMock = new Mock<IScheduleJobManager>();
        _loggerMock = new Mock<ILogger<CreateScheduleHandler>>();

        _handler = new CreateScheduleHandler(
            _scheduleRepoMock.Object,
            _botRegRepoMock.Object,
            _accountRepoMock.Object,
            _jobManagerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidInput_CreatesScheduleAndRegistersJob()
    {
        var botReg = new BotRegistration { Id = Guid.NewGuid(), BotName = "TestBot", Category = "Test" };
        var account = new SocialAccount { Id = Guid.NewGuid(), Name = "TestPage", Platform = Platform.Facebook };
        var dto = new CreateScheduleDto(botReg.Id, account.Id, "0 9 * * *");

        _botRegRepoMock.Setup(r => r.GetByIdAsync(botReg.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(botReg);
        _accountRepoMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _scheduleRepoMock.Setup(r => r.AddAsync(It.IsAny<ScheduleConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScheduleConfig s, CancellationToken _) => s);

        var result = await _handler.Handle(new CreateScheduleCommand(dto), CancellationToken.None);

        result.BotName.Should().Be("TestBot");
        result.AccountName.Should().Be("TestPage");
        result.CronExpression.Should().Be("0 9 * * *");
        _jobManagerMock.Verify(j => j.RegisterOrUpdateRecurringJob(It.IsAny<ScheduleConfig>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BotNotFound_ThrowsInvalidOperation()
    {
        var dto = new CreateScheduleDto(Guid.NewGuid(), Guid.NewGuid(), "0 9 * * *");
        _botRegRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotRegistration?)null);

        var act = () => _handler.Handle(new CreateScheduleCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Bot registration*not found*");
    }

    [Fact]
    public async Task Handle_AccountNotFound_ThrowsInvalidOperation()
    {
        var botReg = new BotRegistration { Id = Guid.NewGuid(), BotName = "TestBot" };
        var dto = new CreateScheduleDto(botReg.Id, Guid.NewGuid(), "0 9 * * *");

        _botRegRepoMock.Setup(r => r.GetByIdAsync(botReg.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(botReg);
        _accountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SocialAccount?)null);

        var act = () => _handler.Handle(new CreateScheduleCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Social account*not found*");
    }
}
