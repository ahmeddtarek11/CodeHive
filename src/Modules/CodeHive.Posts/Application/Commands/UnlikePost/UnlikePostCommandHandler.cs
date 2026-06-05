using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.UnlikePost;

public sealed class UnlikePostCommandHandler : ICommandHandler<UnlikePostCommand>
{
    private readonly CodeHiveDbContext _dbContext;

    public UnlikePostCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UnlikePostCommand request, CancellationToken cancellationToken)
    {
        var postLike = await _dbContext.Set<PostLike>()
            .FirstOrDefaultAsync(
                like => like.PostId == request.PostId && like.UserId == request.UserId,
                cancellationToken);

        if (postLike is null)
        {
            return Result.Fail(PostErrors.NotLiked);
        }

        _dbContext.Set<PostLike>().Remove(postLike);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
