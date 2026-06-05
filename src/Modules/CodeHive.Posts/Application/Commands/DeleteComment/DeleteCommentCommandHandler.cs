using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.DeleteComment;

public sealed class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand>
{
    private readonly CodeHiveDbContext _dbContext;

    public DeleteCommentCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _dbContext.Set<Comment>()
            .FirstOrDefaultAsync(
                currentComment => currentComment.Id == request.CommentId &&
                                  currentComment.PostId == request.PostId,
                cancellationToken);

        if (comment is null)
        {
            return Result.Fail(Error.NotFound("Comment"));
        }

        var post = await _dbContext.Set<Post>()
            .FirstOrDefaultAsync(currentPost => currentPost.Id == request.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Fail(PostErrors.NotFound);
        }

        if (request.RequestingUserId != comment.AuthorId &&
            request.RequestingUserId != post.AuthorId)
        {
            return Result.Fail(PostErrors.NotAuthor);
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
