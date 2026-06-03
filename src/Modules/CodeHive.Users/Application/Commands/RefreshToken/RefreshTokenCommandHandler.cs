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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RefreshTokenEntity = CodeHive.Users.Domain.Data.Entities.RefreshToken;

namespace CodeHive.Users.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthTokensDto>
{
    private readonly CodeHiveDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        CodeHiveDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var rawRefreshToken = request.RefreshToken.Trim();
        var tokenHash = HashRefreshToken(rawRefreshToken);

        _logger.LogInformation("Refresh token rotation started.");

        var currentToken = await _dbContext.Set<RefreshTokenEntity>()
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (currentToken is null)
        {
            _logger.LogWarning("Refresh token rejected: token not found.");

            return UserErrors.TokenInvalid;
        }

        if (currentToken.IsRevoked)
        {
            _logger.LogWarning(
                "Potential refresh token reuse detected. Revoking all tokens for UserId={UserId}, TokenId={TokenId}.",
                currentToken.UserId,
                currentToken.Id);

            var userTokens = await _dbContext.Set<RefreshTokenEntity>()
                .Where(token => token.UserId == currentToken.UserId)
                .ToListAsync(cancellationToken);

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return UserErrors.TokenInvalid;
        }

        if (currentToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Refresh token expired for UserId={UserId}, TokenId={TokenId}, ExpiresAt={ExpiresAt}.",
                currentToken.UserId,
                currentToken.Id,
                currentToken.ExpiresAt);

            return UserErrors.TokenExpired;
        }

        var user = await _userManager.FindByIdAsync(currentToken.UserId.ToString());
        if (user is null)
        {
            _logger.LogError(
                "Refresh token belongs to a missing user. Revoking token and rejecting request. UserId={UserId}, TokenId={TokenId}.",
                currentToken.UserId,
                currentToken.Id);

            currentToken.IsRevoked = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UserErrors.TokenInvalid;
        }

        currentToken.IsRevoked = true;

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email ?? string.Empty, roles);
        var newRefreshToken = _tokenService.CreateRefreshToken();

        var newToken = new RefreshTokenEntity
        {
            UserId = currentToken.UserId,
            TokenHash = HashRefreshToken(newRefreshToken.Value),
            ExpiresAt = newRefreshToken.ExpiresAtUtc
        };

        currentToken.ReplacedBy = newToken.Id;
        _dbContext.Set<RefreshTokenEntity>().Add(newToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Refresh token rotated successfully. UserId={UserId}, PreviousTokenId={PreviousTokenId}, NewTokenId={NewTokenId}, AccessTokenExpiresAt={AccessTokenExpiresAtUtc}.",
            user.Id,
            currentToken.Id,
            newToken.Id,
            accessToken.ExpiresAtUtc);

        return new AuthTokensDto(
            accessToken.Value,
            newRefreshToken.Value,
            accessToken.ExpiresAtUtc);
    }

    private static string HashRefreshToken(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
