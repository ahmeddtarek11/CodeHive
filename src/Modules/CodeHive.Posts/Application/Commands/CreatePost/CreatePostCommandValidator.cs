using CodeHive.Posts.Domain.Entities;
using FluentValidation;

namespace CodeHive.Posts.Application.Commands.CreatePost;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(command => command.Type)
            .Must(type => type.HasValue && Enum.IsDefined(typeof(PostType), type.Value))
            .WithMessage("Type is required.");

        RuleFor(command => command.Content)
            .Must(content => !string.IsNullOrWhiteSpace(content))
            .WithMessage("Content is required.");

        When(command => command.Type == PostType.Short, () =>
        {
            RuleFor(command => command.Content)
                .MaximumLength(5000)
                .WithMessage("Short posts can be at most 5000 characters long.");
        });

        When(command => command.Type == PostType.Blog, () =>
        {
            RuleFor(command => command.Title)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage("Title is required for blog posts.");
        });

        When(command => command.Title is not null, () =>
        {
            RuleFor(command => command.Title!)
                .MaximumLength(200)
                .WithMessage("Title must be at most 200 characters.");
        });

        When(command => command.Type == PostType.Snippet, () =>
        {
            RuleFor(command => command.Language)
                .Must(language => !string.IsNullOrWhiteSpace(language))
                .WithMessage("Language is required for snippets.");
        });

        RuleFor(command => command.Tags)
            .Must(tags => tags is null || tags.Length <= 5)
            .WithMessage("A post can have at most 5 tags.");

        When(command => command.Tags is not null, () =>
        {
            RuleForEach(command => command.Tags!)
                .Must(tag =>
                    !string.IsNullOrWhiteSpace(tag) &&
                    tag.Length <= 30 &&
                    tag == tag.ToLowerInvariant())
                .WithMessage("Tags must be lowercase and at most 30 characters long.");
        });
    }
}
