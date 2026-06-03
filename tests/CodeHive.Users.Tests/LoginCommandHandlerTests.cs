using CodeHive.Infrastructure.Identity;
using CodeHive.Shared.Interfaces.Identity;
using CodeHive.Users.Application.Commands.Login;
using CodeHive.Users.Dtos;
using CodeHive.Users.Domain.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using CodeHive.Infrastructure.Data;

namespace CodeHive.Users.Tests;

public sealed class LoginCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly CodeHiveDbContext _dbContext;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userManager = TestHelpers.CreateUserManager();
        _tokenService = Substitute.For<ITokenService>();
        _dbContext = TestHelpers.CreateDbContext();
        var logger = Substitute.For<ILogger<LoginCommandHandler>>();

        _handler = new LoginCommandHandler(_userManager, _tokenService, _dbContext, logger);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsInvalidCredentials()
    {
        var command = new LoginCommand("missing@test.com", "Password123!");

        _userManager.FindByEmailAsync("missing@test.com")
            .Returns((ApplicationUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidCredentials, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ReturnsInvalidCredentials()
    {
        var command = new LoginCommand("user@test.com", "WrongPassword!");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            UserName = "user"
        };

        _userManager.FindByEmailAsync("user@test.com")
            .Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password)
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidCredentials, result.Error);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsTokens()
    {
        var command = new LoginCommand("user@test.com", "Password123!");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            UserName = "user"
        };
        var expectedAccessToken = new AccessToken("access-token-value", DateTime.UtcNow.AddMinutes(15));
        var expectedRefreshToken = new RefreshToken("refresh-token-value", DateTime.UtcNow.AddDays(7));

        _userManager.FindByEmailAsync("user@test.com")
            .Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password)
            .Returns(true);
        _userManager.GetRolesAsync(user)
            .Returns(new List<string> { "User" });
        _tokenService.GenerateAccessToken(user.Id, user.Email!, Arg.Any<IEnumerable<string>>())
            .Returns(expectedAccessToken);
        _tokenService.CreateRefreshToken()
            .Returns(expectedRefreshToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.AccessToken);
        Assert.NotNull(result.Value.RefreshToken);
        Assert.NotEqual(string.Empty, result.Value.AccessToken);
        Assert.NotEqual(string.Empty, result.Value.RefreshToken);
        Assert.Equal(expectedAccessToken.ExpiresAtUtc, result.Value.AccessTokenExpiry);
    }
}
