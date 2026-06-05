using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.BookmarkPost;

public sealed record BookmarkPostCommand(
    Guid PostId,
    Guid UserId
) : ICommand;
