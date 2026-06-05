using FluentValidation;

namespace CodeHive.Posts.Application.Commands.AddComment;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
