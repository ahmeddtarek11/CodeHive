using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
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
        var postExists = await _dbContext.Set<Post>()
            .AnyAsync(post => post.Id == request.PostId, cancellationToken);

        if (!postExists)
        {
            return PostErrors.NotFound;
        }

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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
