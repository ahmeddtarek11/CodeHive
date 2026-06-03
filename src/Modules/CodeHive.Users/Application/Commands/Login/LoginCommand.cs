using CodeHive.Infrastructure.Identity;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Dtos;

namespace CodeHive.Users.Application.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : ICommand<AuthTokensDto>;
