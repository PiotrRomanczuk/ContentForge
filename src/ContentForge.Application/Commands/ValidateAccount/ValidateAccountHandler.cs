using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContentForge.Application.Commands.ValidateAccount;

public class ValidateAccountHandler : IRequestHandler<ValidateAccountCommand, ValidateAccountResultDto>
{
    private readonly ISocialAccountRepository _accountRepository;
    private readonly IPlatformAdapterFactory _adapterFactory;
    private readonly ILogger<ValidateAccountHandler> _logger;

    public ValidateAccountHandler(
        ISocialAccountRepository accountRepository,
        IPlatformAdapterFactory adapterFactory,
        ILogger<ValidateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _adapterFactory = adapterFactory;
        _logger = logger;
    }

    public async Task<ValidateAccountResultDto> Handle(
        ValidateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.SocialAccountId, cancellationToken)
            ?? throw new InvalidOperationException($"Social account {request.SocialAccountId} not found");

        var adapter = _adapterFactory.GetAdapter(account.Platform);
        var isValid = await adapter.ValidateAccountAsync(account, cancellationToken);

        _logger.LogInformation(
            "Validated account '{Name}' ({Platform}): {Result}",
            account.Name, account.Platform, isValid ? "valid" : "invalid");

        return new ValidateAccountResultDto(
            account.Id, isValid,
            isValid ? null : "Token validation failed or token expired",
            account.TokenExpiresAt);
    }
}
