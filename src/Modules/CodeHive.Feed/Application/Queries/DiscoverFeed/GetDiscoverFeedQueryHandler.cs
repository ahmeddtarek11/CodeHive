using System.Globalization;
using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Domain.Data.Entities;
using Microsoft.EntityFrameworkCore;
using FollowEntity = CodeHive.Users.Domain.Data.Entities.Follow;

namespace CodeHive.Feed.Application.Queries.DiscoverFeed;

public sealed class GetDiscoverFeedQueryHandler : IQueryHandler<GetDiscoverFeedQuery, CursorPage<PostSummaryDto>>
{
    private readonly CodeHiveDbContext _dbContext;

    public GetDiscoverFeedQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CursorPage<PostSummaryDto>>> Handle(
        GetDiscoverFeedQuery request,
        CancellationToken cancellationToken)
    {
        var cursor = ParseCursor(request.Cursor);
        if (request.Cursor is not null && cursor is null)
        {
            return Result.Fail<CursorPage<PostSummaryDto>>(new Error(
                "Pagination.InvalidCursor",
                "Cursor must be a valid ISO 8601 timestamp."));
        }

        var query = _dbContext.Set<Post>()
            .AsNoTracking()
            .AsQueryable();

        if (request.CurrentUserId.HasValue)
        {
            var followeeIds = _dbContext.Set<FollowEntity>()
                .AsNoTracking()
                .Where(follow => follow.FollowerId == request.CurrentUserId.Value)
                .Select(follow => follow.FolloweeId);

            query = query.Where(post => !followeeIds.Contains(post.AuthorId));
        }

        if (cursor.HasValue)
        {
            query = query.Where(post => post.CreatedAt < cursor.Value);
        }

        var rows = await query
            .Join(
                _dbContext.Users.AsNoTracking(),
                post => post.AuthorId,
                author => author.Id,
                (post, author) => new DiscoverFeedRow(
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
                    post.CreatedAt))
            .OrderByDescending(row => row.CreatedAt)
            .Take(request.Limit + 1)
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

    private sealed record DiscoverFeedRow(PostSummaryDto Post, DateTime CreatedAt);
}
