using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Shared.Interfaces.Identity;
using CodeHive.Users.Domain.Errors;
using CodeHive.Users.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RefreshTokenEntity = CodeHive.Users.Domain.Data.Entities.RefreshToken;

namespace CodeHive.Users.Application.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthTokensDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly CodeHiveDbContext _dbContext;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        CodeHiveDbContext dbContext,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<AuthTokensDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        _logger.LogInformation(
            "Login command started for Email={Email}.",
            email);

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning(
                "Login rejected for Email={Email}: invalid credentials.",
                email);

            return UserErrors.InvalidCredentials;
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning(
                "Login rejected for Email={Email}, UserId={UserId}: invalid credentials.",
                email,
                user.Id);

            return UserErrors.InvalidCredentials;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email ?? email, roles);
        var refreshToken = _tokenService.CreateRefreshToken();
        var refreshTokenHash = HashRefreshToken(refreshToken.Value);

        _dbContext.Set<RefreshTokenEntity>().Add(new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshToken.ExpiresAtUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Login succeeded for UserId={UserId}, Email={Email}. AccessTokenExpiresAt={AccessTokenExpiresAtUtc}.",
            user.Id,
            user.Email ?? email,
            accessToken.ExpiresAtUtc);

        return new AuthTokensDto(
            accessToken.Value,
            refreshToken.Value,
            accessToken.ExpiresAtUtc);
    }

    private static string HashRefreshToken(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
