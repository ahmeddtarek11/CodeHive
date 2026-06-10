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

        // Phase 1: fetch flat rows (post + author only) ordered by CreatedAt in SQL.
        // The full PostSummaryDto projection (with tag/like/comment subqueries) is done
        // AFTER ToListAsync so EF never tries to translate it.
        var flatRows = await query
            .Where(p => !p.IsDeleted)
            .Join(
                _dbContext.Users.AsNoTracking(),
                post   => post.AuthorId,
                author => author.Id,
                (post, author) => new
                {
                    post.Id,
                    post.AuthorId,
                    post.Title,
                    post.Content,
                    post.CreatedAt,
                    AuthorUsername = author.UserName!,
                    author.AvatarUrl
                })
            .OrderByDescending(r => r.CreatedAt)
            .Take(request.Limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore  = flatRows.Count > request.Limit;
        var pageRows = flatRows.Take(request.Limit).ToList();

        // Phase 2: enrich each row in memory with tags, like counts, comment counts.
        var postIds = pageRows.Select(r => r.Id).ToList();

        var tagsByPost = await _dbContext.Set<PostTag>().AsNoTracking()
            .Where(pt => postIds.Contains(pt.PostId))
            .Join(
                _dbContext.Set<Tag>().AsNoTracking(),
                pt  => pt.TagId,
                tag => tag.Id,
                (pt, tag) => new { pt.PostId, tag.Name })
            .ToListAsync(cancellationToken);

        var likesByPost = await _dbContext.Set<PostLike>().AsNoTracking()
            .Where(pl => postIds.Contains(pl.PostId))
            .GroupBy(pl => pl.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var commentsByPost = await _dbContext.Set<Comment>().AsNoTracking()
            .Where(c => !c.IsDeleted && postIds.Contains(c.PostId))
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var tagsLookup    = tagsByPost.ToLookup(x => x.PostId, x => x.Name);
        var likesLookup   = likesByPost.ToDictionary(x => x.PostId, x => x.Count);
        var commentsLookup = commentsByPost.ToDictionary(x => x.PostId, x => x.Count);

        var dtos = pageRows.Select(r => new PostSummaryDto(
            r.Id,
            r.AuthorUsername,
            r.AvatarUrl,
            r.Title,
            r.Content.Length > 200 ? r.Content[..200] : r.Content,
            tagsLookup[r.Id].OrderBy(t => t).ToArray(),
            likesLookup.GetValueOrDefault(r.Id),
            commentsLookup.GetValueOrDefault(r.Id),
            r.CreatedAt
        )).ToList();

        var nextCursor = hasMore && dtos.Count > 0
            ? pageRows[^1].CreatedAt.ToString("O")
            : null;

        return new CursorPage<PostSummaryDto>(dtos, nextCursor, hasMore);
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
