using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Shared;
using CodeHive.Shared.Interfaces.Identity;
using CodeHive.Users.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RefreshTokenEntity = CodeHive.Users.Domain.Data.Entities.RefreshToken;

namespace CodeHive.Users.Domain;


public class ExternalAuthService : IExternalAuthService
{

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService               _tokenService;
    private readonly CodeHiveDbContext           _dbContext;
    private readonly IOptions<JwtSettings>       _jwtSettings;

    public ExternalAuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        CodeHiveDbContext db,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager  = userManager;
        _tokenService = tokenService;
        _dbContext = db;
        _jwtSettings  = jwtSettings;
    }

    public async Task<Result<AuthTokensDto>> HandleExternalLoginAsync(string LoginProvider, string ProviderKey, string Email, string DisplayName, string? AvatarUrl, CancellationToken ct = default)
    {
        
        var user = await _userManager.FindByLoginAsync(LoginProvider , ProviderKey );

        if(user is null)
        {
            user = await _userManager.FindByEmailAsync(Email);

            if(user is null)
            {
                user = new ApplicationUser
                {
                    Email = Email ,
                    UserName = await GenerateUniqueUsernameAsync(Email) ,
                    DisplayName = DisplayName ,
                    AvatarUrl = AvatarUrl ,
                    IsEmailVerified = true

                };

                var CreationResult = await _userManager.CreateAsync(user);
                    if (!CreationResult.Succeeded)
                    return new Error("OAuth.CreateFailed", CreationResult.Errors.First().Description);

            }

            await _userManager.AddLoginAsync(user , new UserLoginInfo(LoginProvider , ProviderKey , LoginProvider));
        }

        var UserRoles = await   _userManager.GetRolesAsync(user);


        var accessToken = _tokenService.GenerateAccessToken(user.Id ,user.Email! , UserRoles);
        var (rawToken, Expiration) = _tokenService.CreateRefreshToken();
        
        _dbContext.Set<RefreshTokenEntity>().Add(new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = HashRefreshToken(rawToken),
            ExpiresAt = Expiration
        });

        await _dbContext.SaveChangesAsync();

        return new AuthTokensDto(accessToken.Value ,rawToken ,Expiration);

        
    }

    private async Task<string> GenerateUniqueUsernameAsync(string email)
    {
        // Take the part before @, remove non-alphanumeric characters
        var baseName = new string(email.Split('@')[0]
            .Where(char.IsLetterOrDigit).ToArray()).ToLower();

        if (baseName.Length < 3) baseName = "user" + baseName;

        // Ensure uniqueness by appending a number if needed
        var candidate = baseName;
        var i = 1;
        while (await _userManager.FindByNameAsync(candidate) is not null)
            candidate = $"{baseName}{i++}";

        return candidate;
    }

     private static string HashRefreshToken(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }


    
}
