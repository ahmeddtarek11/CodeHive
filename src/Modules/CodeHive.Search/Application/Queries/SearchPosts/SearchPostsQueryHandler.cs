using System.Globalization;
using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Search.Application.Queries.SearchPosts;

public sealed class SearchPostsQueryHandler : IQueryHandler<SearchPostsQuery, CursorPage<SearchPostSummaryDto>>
{
    private readonly CodeHiveDbContext _dbContext;

    public SearchPostsQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CursorPage<SearchPostSummaryDto>>> Handle(
        SearchPostsQuery request,
        CancellationToken cancellationToken)
    {
        var queryText = request.Query.Trim();

        var cursor = ParseCursor(request.Cursor);
        if (request.Cursor is not null && cursor is null)
        {
            return Result.Fail<CursorPage<SearchPostSummaryDto>>(new Error(
                "Pagination.InvalidCursor",
                "Cursor must be a valid ISO 8601 timestamp."));
        }

        var query = _dbContext.Set<Post>()
            .AsNoTracking()
            .Where(post =>
                EF.Functions.ILike(post.Content, $"%{queryText}%") ||
                (post.Title != null && EF.Functions.ILike(post.Title, $"%{queryText}%")));

        if (cursor.HasValue)
        {
            query = query.Where(post => post.CreatedAt < cursor.Value);
        }

        var rows = await query
            .Join(
                _dbContext.Users.AsNoTracking(),
                post => post.AuthorId,
                author => author.Id,
                (post, author) => new { post, author })
            .OrderByDescending(x => x.post.CreatedAt)
            .Take(request.Limit + 1)
            .Select(x => new PostSearchRow(
                new SearchPostSummaryDto(
                    x.post.Id,
                    x.author.UserName!,
                    x.author.AvatarUrl,
                    x.post.Title,
                    x.post.Content.Length > 200
                        ? x.post.Content.Substring(0, 200)
                        : x.post.Content,
                    _dbContext.Set<PostTag>().AsNoTracking()
                        .Where(postTag => postTag.PostId == x.post.Id)
                        .Join(
                            _dbContext.Set<Tag>().AsNoTracking(),
                            postTag => postTag.TagId,
                            tag => tag.Id,
                            (_, tag) => tag.Name)
                        .OrderBy(tagName => tagName)
                        .ToArray(),
                    _dbContext.Set<PostLike>().AsNoTracking().Count(postLike => postLike.PostId == x.post.Id),
                    _dbContext.Set<Comment>().AsNoTracking().Count(comment => comment.PostId == x.post.Id),
                    x.post.CreatedAt),
                x.post.CreatedAt))
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > request.Limit;
        var pageItems = rows.Take(request.Limit).ToList();
        var nextCursor = hasMore && pageItems.Count > 0
            ? pageItems[^1].CreatedAt.ToString("O")
            : null;

        return new CursorPage<SearchPostSummaryDto>(
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

    private sealed record PostSearchRow(SearchPostSummaryDto Post, DateTime CreatedAt);
}
