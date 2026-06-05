using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.BookmarkPost;

public sealed class BookmarkPostCommandHandler : ICommandHandler<BookmarkPostCommand>
{
    private readonly CodeHiveDbContext _dbContext;

    public BookmarkPostCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(BookmarkPostCommand request, CancellationToken cancellationToken)
    {
        var postExists = await _dbContext.Set<Post>()
            .AnyAsync(post => post.Id == request.PostId, cancellationToken);

        if (!postExists)
        {
            return Result.Fail(PostErrors.NotFound);
        }

        var alreadyBookmarked = await _dbContext.Set<Bookmark>()
            .AnyAsync(bookmark => bookmark.PostId == request.PostId && bookmark.UserId == request.UserId, cancellationToken);

        if (!alreadyBookmarked)
        {
            _dbContext.Set<Bookmark>().Add(new Bookmark
            {
                PostId = request.PostId,
                UserId = request.UserId
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
