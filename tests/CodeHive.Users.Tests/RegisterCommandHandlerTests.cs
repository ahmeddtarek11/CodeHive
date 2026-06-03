using CodeHive.Infrastructure.Identity;
using CodeHive.Users.Application.Commands.Register;
using CodeHive.Users.Domain.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CodeHive.Users.Tests;

public sealed class RegisterCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userManager = TestHelpers.CreateUserManager();
        var logger = Substitute.For<ILogger<RegisterCommandHandler>>();
        _handler = new RegisterCommandHandler(_userManager, logger);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsEmailTaken()
    {
        var command = new RegisterCommand("taken@test.com", "new_user", "Password123!", "Display Name");

        _userManager.FindByEmailAsync("taken@test.com")
            .Returns(new ApplicationUser());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.EmailTaken, result.Error);
    }

    [Fact]
    public async Task Handle_WhenUsernameAlreadyExists_ReturnsUsernameTaken()
    {
        var command = new RegisterCommand("new@test.com", "taken_user", "Password123!", "Display Name");

        _userManager.FindByEmailAsync("new@test.com")
            .Returns((ApplicationUser?)null);
        _userManager.FindByNameAsync("taken_user")
            .Returns(new ApplicationUser());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.UsernameTaken, result.Error);
    }

    [Fact]
    public async Task Handle_WhenIdentityCreationFails_ReturnsIdentityMessage()
    {
        var command = new RegisterCommand("new@test.com", "new_user", "Password123!", "Display Name");
        var identityError = new IdentityError
        {
            Code = "PasswordTooWeak",
            Description = "Password does not meet complexity requirements."
        };

        _userManager.FindByEmailAsync("new@test.com")
            .Returns((ApplicationUser?)null);
        _userManager.FindByNameAsync("new_user")
            .Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Failed(identityError));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.CreateFailed", result.Error.Code);
        Assert.Equal(identityError.Description, result.Error.Description);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsGuid()
    {
        var command = new RegisterCommand("new@test.com", "new_user", "Password123!", "Display Name");
        var expectedUserId = Guid.NewGuid();

        _userManager.FindByEmailAsync("new@test.com")
            .Returns((ApplicationUser?)null);
        _userManager.FindByNameAsync("new_user")
            .Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(callInfo =>
            {
                var user = callInfo.ArgAt<ApplicationUser>(0);
                user.Id = expectedUserId;
                return IdentityResult.Success;
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUserId, result.Value);
    }
}
