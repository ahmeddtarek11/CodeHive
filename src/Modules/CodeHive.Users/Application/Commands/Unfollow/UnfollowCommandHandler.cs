using CodeHive.Infrastructure.Caching;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using FollowEntity = CodeHive.Users.Domain.Data.Entities.Follow;

namespace CodeHive.Users.Application.Commands.Unfollow;

public sealed class UnfollowCommandHandler : ICommandHandler<UnfollowCommand>
{
    private readonly CodeHiveDbContext _dbContext;
    private readonly ICacheService _cache;

    public UnfollowCommandHandler(CodeHiveDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<Result> Handle(UnfollowCommand request, CancellationToken cancellationToken)
    {
        var follow = await _dbContext.Set<FollowEntity>()
            .FirstOrDefaultAsync(
                f => f.FollowerId == request.FollowerId &&
                     f.FolloweeId == request.FolloweeId,
                cancellationToken);

        if (follow is null)
        {
            return Result.Fail(Error.NotFound("Follow"));
        }

        _dbContext.Set<FollowEntity>().Remove(follow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate follower's feed (they no longer see the followee's posts)
        await _cache.RemoveByPrefixAsync(CacheKeys.FeedPrefix(request.FollowerId), cancellationToken);

        // Invalidate both profile caches — follower count and following count both changed
        var usernames = await _dbContext.Users
            .Where(u => u.Id == request.FollowerId || u.Id == request.FolloweeId)
            .Select(u => u.UserName!)
            .ToListAsync(cancellationToken);

        foreach (var username in usernames)
            await _cache.RemoveAsync(CacheKeys.UserProfile(username.ToLowerInvariant()), cancellationToken);

        return Result.Ok();
    }
}
