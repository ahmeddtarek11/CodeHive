using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Domain.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FollowEntity = CodeHive.Users.Domain.Data.Entities.Follow;

namespace CodeHive.Users.Application.Commands.Follow;

public sealed class FollowCommandHandler : ICommandHandler<FollowCommand>
{
    private readonly CodeHiveDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public FollowCommandHandler(
        CodeHiveDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
