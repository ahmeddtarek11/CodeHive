using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.DeletePost;

public sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand>
{
    private readonly CodeHiveDbContext _dbContext;

    public DeletePostCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _dbContext.Set<Post>()
            .FirstOrDefaultAsync(currentPost => currentPost.Id == request.Id, cancellationToken);

        if (post is null)
        {
            return Result.Fail(PostErrors.NotFound);
        }

        if (post.AuthorId != request.RequestingUserId)
        {
            return Result.Fail(PostErrors.NotAuthor);
        }

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
