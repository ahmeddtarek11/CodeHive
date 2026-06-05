using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.UnlikePost;

public sealed record UnlikePostCommand(
    Guid PostId,
    Guid UserId
) : ICommand;
