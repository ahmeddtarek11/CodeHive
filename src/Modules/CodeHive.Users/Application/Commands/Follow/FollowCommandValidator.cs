using FluentValidation;

namespace CodeHive.Users.Application.Commands.Follow;

public sealed class FollowCommandValidator : AbstractValidator<FollowCommand>
{
    public FollowCommandValidator()
    {
        RuleFor(command => command.FollowerId).NotEmpty();
        RuleFor(command => command.FolloweeId).NotEmpty();
    }
}
