using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Infrastructure.Identity;
using CodeHive.Users.Domain.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CodeHive.Users.Application.Commands.Register;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Guid>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var username = request.Username.Trim().ToLowerInvariant();
        var displayName = request.DisplayName.Trim();

        _logger.LogInformation(
            "Register command started for Username={Username}, Email={Email}.",
            username,
            email);

        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
        {
            _logger.LogWarning(
                "Registration rejected because email is already taken. Email={Email}.",
                email);

            return UserErrors.EmailTaken;
        }

        var existingByUsername = await _userManager.FindByNameAsync(username);
        if (existingByUsername is not null)
        {
            _logger.LogWarning(
                "Registration rejected because username is already taken. Username={Username}.",
                username);

            return UserErrors.UsernameTaken;
        }

        var user = new ApplicationUser
        {
            Email = email,
            UserName = username,
            DisplayName = displayName,
            IsEmailVerified = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var identityError = createResult.Errors.FirstOrDefault();
            var firstError = createResult.Errors.FirstOrDefault()?.Description
                ?? "Failed to create user.";

            _logger.LogWarning(
                "Registration failed for Username={Username}, Email={Email}. IdentityErrorCode={IdentityErrorCode}, IdentityErrorDescription={IdentityErrorDescription}.",
                username,
                email,
                identityError?.Code ?? string.Empty,
                firstError);

            return new Error("User.CreateFailed", firstError);
        }

        _logger.LogInformation(
            "User registered successfully. UserId={UserId}, Username={Username}, Email={Email}.",
            user.Id,
            username,
            email);

        return user.Id;
    }
}
