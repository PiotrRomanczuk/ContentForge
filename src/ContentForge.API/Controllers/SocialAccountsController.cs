using ContentForge.Application.Commands.ValidateAccount;
using ContentForge.Application.DTOs;
using ContentForge.Application.Services;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentForge.API.Controllers;

// CRUD controller for social media accounts (Facebook, Instagram, etc.).
// Access tokens are encrypted at rest using Data Protection API.
[ApiController]
[Route("api/social-accounts")]
[Authorize]
public class SocialAccountsController : ControllerBase
{
    private readonly ISocialAccountRepository _accountRepository;
    private readonly IMediator _mediator;
    private readonly ITokenEncryptionService _tokenEncryption;
    private readonly ILogger<SocialAccountsController> _logger;

    public SocialAccountsController(
        ISocialAccountRepository accountRepository,
        IMediator mediator,
        ITokenEncryptionService tokenEncryption,
        ILogger<SocialAccountsController> logger)
    {
        _accountRepository = accountRepository;
        _mediator = mediator;
        _tokenEncryption = tokenEncryption;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SocialAccountDto>>> GetAll(
        [FromQuery] string? platform = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving social accounts (platform filter: {Platform})", platform ?? "all");
        var accounts = platform != null && Enum.TryParse<Platform>(platform, true, out var p)
            ? await _accountRepository.GetByPlatformAsync(p, cancellationToken)
            : await _accountRepository.GetAllAsync(cancellationToken);

        return Ok(accounts.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SocialAccountDto>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null)
        {
            _logger.LogDebug("Social account {Id} not found", id);
            return NotFound();
        }
        return Ok(ToDto(account));
    }

    [HttpPost]
    public async Task<ActionResult<SocialAccountDto>> Create(
        [FromBody] CreateSocialAccountDto dto,
        CancellationToken cancellationToken)
    {
        var account = new SocialAccount
        {
            Name = dto.Name,
            Platform = Enum.Parse<Platform>(dto.Platform, ignoreCase: true),
            ExternalId = dto.ExternalId,
            // SECURITY: Encrypt token before storing in DB
            AccessToken = _tokenEncryption.Encrypt(dto.AccessToken),
            TokenExpiresAt = dto.TokenExpiresAt,
            Metadata = dto.Metadata ?? new Dictionary<string, string>()
        };

        var saved = await _accountRepository.AddAsync(account, cancellationToken);
        _logger.LogInformation("Created social account '{Name}' ({Platform})", saved.Name, saved.Platform);

        return CreatedAtAction(nameof(GetById), new { id = saved.Id }, ToDto(saved));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SocialAccountDto>> Update(
        Guid id, [FromBody] UpdateSocialAccountDto dto,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null) return NotFound();

        _logger.LogInformation("Updating social account {Id}", id);
        // Only update provided fields (null = skip). Like PATCH semantics.
        if (dto.Name != null) account.Name = dto.Name;
        if (dto.AccessToken != null) account.AccessToken = _tokenEncryption.Encrypt(dto.AccessToken);
        if (dto.TokenExpiresAt.HasValue) account.TokenExpiresAt = dto.TokenExpiresAt;
        if (dto.IsActive.HasValue) account.IsActive = dto.IsActive.Value;

        await _accountRepository.UpdateAsync(account, cancellationToken);
        return Ok(ToDto(account));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Deactivate(
        Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null) return NotFound();

        // Soft-delete: set IsActive = false instead of deleting the row.
        account.IsActive = false;
        await _accountRepository.UpdateAsync(account, cancellationToken);

        _logger.LogInformation("Deactivated social account '{Name}'", account.Name);
        return NoContent();
    }

    /// <summary>
    /// Validate the account's access token against the platform API.
    /// </summary>
    [HttpPost("{id:guid}/validate")]
    public async Task<ActionResult<ValidateAccountResultDto>> Validate(
        Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating social account {Id}", id);
        var result = await _mediator.Send(
            new ValidateAccountCommand(id), cancellationToken);
        return Ok(result);
    }

    // Maps entity to DTO — excludes sensitive fields (AccessToken).
    private static SocialAccountDto ToDto(SocialAccount a) => new(
        a.Id, a.Name, a.Platform, a.ExternalId,
        a.IsActive, a.TokenExpiresAt);
}
