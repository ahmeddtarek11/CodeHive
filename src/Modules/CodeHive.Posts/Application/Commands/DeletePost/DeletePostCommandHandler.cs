using CodeHive.Infrastructure.Caching;
using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.DeletePost;

public sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand>
{
    private readonly CodeHiveDbContext _dbContext;
    private readonly ICacheService _cache;

    public DeletePostCommandHandler(CodeHiveDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
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

        // Invalidate cached feeds of all followers so the deleted post disappears immediately
        // var followerIds = await _dbContext.Set<Follow>()
        //     .Where(f => f.FolloweeId == post.AuthorId)
        //     .Select(f => f.FollowerId)
        //     .ToListAsync(cancellationToken);


        // Direct query is used to guarantee immediate cache invalidation within the same transaction boundary
        // Event-driven approach is avoided here to prevent stale feed visibility caused by async propagation delays
        // invalidating data this way is valid in this stage , but when a user has 100,000 follower for example 
        // this becomes slow 
        // ------- to be implemented fanout problem background job in the next stage -------- // 

                var followerIds = await _dbContext.Database
            .SqlQuery<Guid>(
                $"""
                SELECT "FollowerId"
                FROM "Follow"
                WHERE "FolloweeId" = {request.RequestingUserId}
                """)
            .ToListAsync(cancellationToken);

            foreach (var followerId in followerIds)
                await _cache.RemoveByPrefixAsync(CacheKeys.FeedPrefix(followerId), cancellationToken);

        return Result.Ok();
    }
}
