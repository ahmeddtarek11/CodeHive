using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.DeleteComment;

public sealed record DeleteCommentCommand(
    Guid PostId,
    Guid CommentId,
    Guid RequestingUserId
) : ICommand;
