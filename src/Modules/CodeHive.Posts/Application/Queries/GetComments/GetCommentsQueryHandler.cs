using System.Globalization;
using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Queries.GetComments;

public sealed class GetCommentsQueryHandler : IQueryHandler<GetCommentsQuery, CursorPage<CommentDto>>
{
    private readonly CodeHiveDbContext _dbContext;

    public GetCommentsQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CursorPage<CommentDto>>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var postExists = await _dbContext.Set<Post>()
            .AsNoTracking()
            .AnyAsync(post => post.Id == request.PostId, cancellationToken);

        if (!postExists)
        {
            return Result.Fail<CursorPage<CommentDto>>(PostErrors.NotFound);
        }

        var cursor = ParseCursor(request.Cursor);
        if (request.Cursor is not null && cursor is null)
        {
            return Result.Fail<CursorPage<CommentDto>>(new Error(
                "Pagination.InvalidCursor",
                "Cursor must be a valid ISO 8601 timestamp."));
        }

        var topLevelRows = await (
            from comment in _dbContext.Set<Comment>().AsNoTracking()
            join author in _dbContext.Users.AsNoTracking() on comment.AuthorId equals author.Id
            where comment.PostId == request.PostId
                  && comment.ParentCommentId == null
                  && (!cursor.HasValue || comment.CreatedAt > cursor.Value)
            orderby comment.CreatedAt ascending
            select new CommentPageRow(
                new CommentDto(
                    comment.Id,
                    new UserSummaryDto(
                        author.Id,
                        author.UserName!,
                        author.DisplayName,
                        author.AvatarUrl),
                    comment.Content,
                    comment.CreatedAt,
                    new List<CommentDto>()),
                comment.CreatedAt))
            .Take(request.Limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = topLevelRows.Count > request.Limit;
        var pageRows = topLevelRows.Take(request.Limit).ToList();
        var topLevelIds = pageRows.Select(row => row.Comment.Id).ToArray();

        if (topLevelIds.Length > 0)
        {
            var replyRows = await (
                from comment in _dbContext.Set<Comment>().AsNoTracking()
                join author in _dbContext.Users.AsNoTracking() on comment.AuthorId equals author.Id
                where comment.PostId == request.PostId
                      && comment.ParentCommentId.HasValue
                      && topLevelIds.Contains(comment.ParentCommentId.Value)
                orderby comment.CreatedAt ascending
                select new ReplyPageRow(
                    comment.ParentCommentId!.Value,
                    new CommentDto(
                        comment.Id,
                        new UserSummaryDto(
                            author.Id,
                            author.UserName!,
                            author.DisplayName,
                            author.AvatarUrl),
                        comment.Content,
                        comment.CreatedAt,
                        new List<CommentDto>())))
                .ToListAsync(cancellationToken);

            var repliesByParent = replyRows
                .GroupBy(row => row.ParentCommentId)
                .ToDictionary(group => group.Key, group => group.Select(row => row.Comment).ToList());

            foreach (var row in pageRows)
            {
                if (repliesByParent.TryGetValue(row.Comment.Id, out var replies))
                {
                    row.Comment.Replies.AddRange(replies);
                }
            }
        }

        var nextCursor = hasMore && pageRows.Count > 0
            ? pageRows[^1].CreatedAt.ToString("O")
            : null;

        return new CursorPage<CommentDto>(
            pageRows.Select(row => row.Comment).ToList(),
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

    private sealed record CommentPageRow(CommentDto Comment, DateTime CreatedAt);
    private sealed record ReplyPageRow(Guid ParentCommentId, CommentDto Comment);
}
