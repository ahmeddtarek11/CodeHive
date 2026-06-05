using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.RemoveBookmark;

public sealed class RemoveBookmarkCommandHandler : ICommandHandler<RemoveBookmarkCommand>
{
    private readonly CodeHiveDbContext _dbContext;

    public RemoveBookmarkCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(RemoveBookmarkCommand request, CancellationToken cancellationToken)
    {
        var bookmark = await _dbContext.Set<Bookmark>()
            .FirstOrDefaultAsync(
                currentBookmark => currentBookmark.PostId == request.PostId &&
                                   currentBookmark.UserId == request.UserId,
                cancellationToken);

        if (bookmark is null)
        {
            return Result.Ok();
        }

        _dbContext.Set<Bookmark>().Remove(bookmark);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
