using FluentValidation;

namespace CodeHive.Users.Application.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .Must(email => !string.IsNullOrWhiteSpace(email))
            .WithMessage("Email is required.")
            .MaximumLength(255)
            .EmailAddress();

        RuleFor(x => x.Username)
            .Must(username => !string.IsNullOrWhiteSpace(username))
            .WithMessage("Username is required.")
            .MaximumLength(30)
            .Matches("^[A-Za-z0-9_.-]+$")
            .WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens.");

        RuleFor(x => x.Password)
            .Must(password => !string.IsNullOrWhiteSpace(password))
            .WithMessage("Password is required.")
            .MaximumLength(128);
           

        RuleFor(x => x.DisplayName)
            .Must(displayName => !string.IsNullOrWhiteSpace(displayName))
            .WithMessage("Display name is required.")
            .MaximumLength(80);
    }
}
