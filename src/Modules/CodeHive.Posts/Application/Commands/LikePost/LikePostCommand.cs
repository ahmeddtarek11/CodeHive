using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.LikePost;

public sealed record LikePostCommand(
    Guid PostId,
    Guid UserId
) : ICommand;
