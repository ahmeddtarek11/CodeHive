using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Posts.PostsEvents;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.AddComment;

public sealed class AddCommentCommandHandler : ICommandHandler<AddCommentCommand, Guid>
{
    private readonly CodeHiveDbContext _dbContext;

    public AddCommentCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var post = await _dbContext.Set<Post>()
            .Select(p => new { p.Id, p.AuthorId })
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post is null)
        {
            return PostErrors.NotFound;
        }

        var commentAuthorUsername = await _dbContext.Users
            .Where(u => u.Id == request.AuthorId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken) ?? "Unknown";

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _dbContext.Set<Comment>()
                .FirstOrDefaultAsync(comment => comment.Id == request.ParentCommentId.Value, cancellationToken);

            if (parentComment is null || parentComment.PostId != request.PostId)
            {
                return Error.NotFound("Comment");
            }

            if (parentComment.ParentCommentId.HasValue)
            {
                return new Error("Comment.TooDeep", "Replies to replies are not allowed");
            }
        }

        var comment = new Comment
        {
            PostId = request.PostId,
            AuthorId = request.AuthorId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content.Trim()
        };

        _dbContext.Set<Comment>().Add(comment);

        comment.RasieDomainEvent(new CommentAddedEvent(comment.AuthorId, commentAuthorUsername, comment.PostId, post.AuthorId, comment.ParentCommentId));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
