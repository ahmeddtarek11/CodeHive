using CodeHive.Infrastructure.Caching;
using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Domain.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FollowEntity = CodeHive.Users.Domain.Data.Entities.Follow;
using CodeHive.Infrastructure.Outbox;
using System.Text.Json;
using CodeHive.Users.UsersEvents;

namespace CodeHive.Users.Application.Commands.Follow;

public sealed class FollowCommandHandler : ICommandHandler<FollowCommand>
{
    private readonly CodeHiveDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICacheService _cache ;

    public FollowCommandHandler(
        CodeHiveDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ICacheService cache)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _cache = cache;
    }

    public async Task<Result> Handle(FollowCommand request, CancellationToken cancellationToken)
    {
        if (request.FollowerId == request.FolloweeId)
        {
            return Result.Fail(UserErrors.CannotFollowSelf);
        }

        var followee = await _userManager.FindByIdAsync(request.FolloweeId.ToString());
        if (followee is null)
        {
            return Result.Fail(UserErrors.NotFound);
        }

        var alreadyFollowing = await _dbContext.Set<FollowEntity>()
            .AnyAsync(
                follow => follow.FollowerId == request.FollowerId &&
                          follow.FolloweeId == request.FolloweeId,
                cancellationToken);

        if (alreadyFollowing)
        {
            return Result.Fail(UserErrors.AlreadyFollowing);
        }

        _dbContext.Set<FollowEntity>().Add(new FollowEntity
        {
            FollowerId = request.FollowerId,
            FolloweeId = request.FolloweeId
        });

        var follower = await _userManager.FindByIdAsync(request.FollowerId.ToString());

        var msg = new UserFollowedEvent(request.FolloweeId, request.FollowerId, follower?.UserName ?? string.Empty);
        _dbContext.Set<OutboxMessage>().Add(new OutboxMessage
        {
            EventType = typeof(UserFollowedEvent).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(msg)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate follower's feed (they now see the followee's posts)
        await _cache.RemoveByPrefixAsync(CacheKeys.FeedPrefix(request.FollowerId), cancellationToken);

        // Invalidate both profile caches — follower count and following count both changed
        if (follower is not null)
            await _cache.RemoveAsync(CacheKeys.UserProfile(follower.UserName!.ToLowerInvariant()), cancellationToken);

        await _cache.RemoveAsync(CacheKeys.UserProfile(followee.UserName!.ToLowerInvariant()), cancellationToken);

        return Result.Ok();
    }
}
