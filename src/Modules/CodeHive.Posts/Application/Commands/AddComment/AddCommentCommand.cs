using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.AddComment;

public sealed record AddCommentCommand(
    Guid PostId,
    Guid? ParentCommentId,
    string Content,
    Guid AuthorId
) : ICommand<Guid>;
