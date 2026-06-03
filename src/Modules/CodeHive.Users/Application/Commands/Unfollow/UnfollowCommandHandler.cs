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

    public UnfollowCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
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

        return Result.Ok();
    }
}
