using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.LikePost;

public sealed class LikePostCommandHandler : ICommandHandler<LikePostCommand>
{
    private readonly CodeHiveDbContext _dbContext;

    public LikePostCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var postExists = await _dbContext.Set<Post>()
            .AnyAsync(post => post.Id == request.PostId, cancellationToken);

        if (!postExists)
        {
            return Result.Fail(PostErrors.NotFound);
        }

        var alreadyLiked = await _dbContext.Set<PostLike>()
            .AnyAsync(postLike => postLike.PostId == request.PostId && postLike.UserId == request.UserId, cancellationToken);

        if (alreadyLiked)
        {
            return Result.Fail(PostErrors.AlreadyLiked);
        }

        _dbContext.Set<PostLike>().Add(new PostLike
        {
            PostId = request.PostId,
            UserId = request.UserId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
