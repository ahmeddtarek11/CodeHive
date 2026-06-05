using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.RemoveBookmark;

public sealed record RemoveBookmarkCommand(
    Guid PostId,
    Guid UserId
) : ICommand;
