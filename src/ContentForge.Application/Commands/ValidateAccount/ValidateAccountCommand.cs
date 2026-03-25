using ContentForge.Application.DTOs;
using MediatR;

namespace ContentForge.Application.Commands.ValidateAccount;

public record ValidateAccountCommand(Guid SocialAccountId) : IRequest<ValidateAccountResultDto>;
