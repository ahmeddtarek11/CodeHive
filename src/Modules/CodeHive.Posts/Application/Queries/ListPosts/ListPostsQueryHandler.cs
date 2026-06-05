using System.Globalization;
using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Queries.ListPosts;

public sealed class ListPostsQueryHandler : IQueryHandler<ListPostsQuery, CursorPage<PostSummaryDto>>
{
    private readonly CodeHiveDbContext _dbContext;

    public ListPostsQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CursorPage<PostSummaryDto>>> Handle(
        ListPostsQuery request,
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
            from post in _dbContext.Set<Post>().AsNoTracking()
            join author in _dbContext.Users.AsNoTracking() on post.AuthorId equals author.Id
            select new
            {
                Post = post,
                Author = author
            };

        if (request.AuthorId.HasValue)
        {
            query = query.Where(row => row.Post.AuthorId == request.AuthorId.Value);
        }

        if (cursor.HasValue)
        {
            query = query.Where(row => row.Post.CreatedAt < cursor.Value);
        }

        var rows = await query
            .OrderByDescending(row => row.Post.CreatedAt)
            .Take(request.Limit + 1)
            .Select(row => new PostSummaryRow(
                new PostSummaryDto(
                    row.Post.Id,
                    row.Author.UserName!,
                    row.Author.AvatarUrl,
                    row.Post.Title,
                    row.Post.Content.Length > 200
                        ? row.Post.Content.Substring(0, 200)
                        : row.Post.Content,
                    _dbContext.Set<PostTag>().AsNoTracking()
                        .Where(postTag => postTag.PostId == row.Post.Id)
                        .Join(
                            _dbContext.Set<Tag>().AsNoTracking(),
                            postTag => postTag.TagId,
                            tag => tag.Id,
                            (_, tag) => tag.Name)
                        .OrderBy(tagName => tagName)
                        .ToArray(),
                    _dbContext.Set<PostLike>().AsNoTracking().Count(postLike => postLike.PostId == row.Post.Id),
                    _dbContext.Set<Comment>().AsNoTracking().Count(comment => comment.PostId == row.Post.Id),
                    row.Post.CreatedAt),
                row.Post.CreatedAt))
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > request.Limit;
        var pageItems = rows.Take(request.Limit).ToList();
        var nextCursor = hasMore && pageItems.Count > 0
            ? pageItems[^1].CreatedAt.ToString("O")
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

    private sealed record PostSummaryRow(PostSummaryDto Post, DateTime CreatedAt);
}
