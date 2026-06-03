using CodeHive.Shared.Cqrs;
using CodeHive.Users.Dtos;

namespace CodeHive.Users.Application.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : ICommand<AuthTokensDto>;
