using System.Globalization;
using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Queries.GetBookmarks;

public sealed class GetBookmarksQueryHandler : IQueryHandler<GetBookmarksQuery, CursorPage<PostSummaryDto>>
{
    private readonly CodeHiveDbContext _dbContext;

    public GetBookmarksQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CursorPage<PostSummaryDto>>> Handle(
        GetBookmarksQuery request,
        CancellationToken cancellationToken)
    {
        var cursor = ParseCursor(request.Cursor);
        if (request.Cursor is not null && cursor is null)
        {
            return Result.Fail<CursorPage<PostSummaryDto>>(new Error(
                "Pagination.InvalidCursor",
                "Cursor must be a valid ISO 8601 timestamp."));
        }

        var query =
            from bookmark in _dbContext.Set<Bookmark>().AsNoTracking()
            join post in _dbContext.Set<Post>().AsNoTracking() on bookmark.PostId equals post.Id
            join author in _dbContext.Users.AsNoTracking() on post.AuthorId equals author.Id
            where bookmark.UserId == request.UserId
                  && (!cursor.HasValue || bookmark.BookmarkedAt < cursor.Value)
            orderby bookmark.BookmarkedAt descending
            select new BookmarkRow(
                new PostSummaryDto(
                    post.Id,
                    author.UserName!,
                    author.AvatarUrl,
                    post.Title,
                    post.Content.Length > 200
                        ? post.Content.Substring(0, 200)
                        : post.Content,
                    _dbContext.Set<PostTag>().AsNoTracking()
                        .Where(postTag => postTag.PostId == post.Id)
                        .Join(
                            _dbContext.Set<Tag>().AsNoTracking(),
                            postTag => postTag.TagId,
                            tag => tag.Id,
                            (_, tag) => tag.Name)
                        .OrderBy(tagName => tagName)
                        .ToArray(),
                    _dbContext.Set<PostLike>().AsNoTracking().Count(postLike => postLike.PostId == post.Id),
                    _dbContext.Set<Comment>().AsNoTracking().Count(comment => comment.PostId == post.Id),
                    post.CreatedAt),
                bookmark.BookmarkedAt);

        var rows = await query
            .Take(request.Limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > request.Limit;
        var pageItems = rows.Take(request.Limit).ToList();
        var nextCursor = hasMore && pageItems.Count > 0
            ? pageItems[^1].BookmarkedAt.ToString("O")
            : null;

        return new CursorPage<PostSummaryDto>(
            pageItems.Select(row => row.Post).ToList(),
            nextCursor,
            hasMore);
    }

    private static DateTime? ParseCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        return DateTimeOffset.TryParse(
            cursor,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed.UtcDateTime
            : null;
    }

    private sealed record BookmarkRow(PostSummaryDto Post, DateTime BookmarkedAt);
}
