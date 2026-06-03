using FluentValidation;

namespace CodeHive.Users.Application.Commands.Unfollow;

public sealed class UnfollowCommandValidator : AbstractValidator<UnfollowCommand>
{
    public UnfollowCommandValidator()
    {
        RuleFor(command => command.FollowerId).NotEmpty();
        RuleFor(command => command.FolloweeId).NotEmpty();
    }
}
