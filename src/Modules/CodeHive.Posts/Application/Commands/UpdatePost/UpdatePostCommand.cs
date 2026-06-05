using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.UpdatePost;

public record UpdatePostCommand(
    Guid Id,
    Guid RequestingUserId,
    string? Title,
    string Content,
    string? Language,
    string[]? Tags
) : ICommand;
