using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Infrastructure.Identity;
using CodeHive.Users.Domain.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using CodeHive.Users.UsersEvents;
using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Outbox;
using System.Text.Json;

namespace CodeHive.Users.Application.Commands.Register;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Guid>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly CodeHiveDbContext _dbContext;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterCommandHandler> logger,
        CodeHiveDbContext dbContext)
    {
        _userManager = userManager;
        _logger = logger;
        _dbContext = dbContext;
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



        user.RasieDomainEvent(new UserRegisteredEvent(user.Id, user.Email!, user.UserName!, user.DisplayName));
        // var msg = new UserRegisteredEvent(user.Id, user.Email!, user.UserName!, user.DisplayName);
        // _dbContext.Set<OutboxMessage>().Add(new OutboxMessage
        // {
        //     EventType = typeof(UserRegisteredEvent).AssemblyQualifiedName!,
        //     Payload = JsonSerializer.Serialize(msg)
        // });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
