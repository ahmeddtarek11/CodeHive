using FluentValidation;

namespace CodeHive.Users.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.DisplayName)
            .Must(displayName => !string.IsNullOrWhiteSpace(displayName))
            .MaximumLength(80);
    }
}
