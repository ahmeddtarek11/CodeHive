using System.Globalization;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Search.Application.Queries.SearchUsers;

public sealed class SearchUsersQueryHandler : IQueryHandler<SearchUsersQuery, CursorPage<UserSearchSummaryDto>>
{
    private readonly CodeHiveDbContext _dbContext;

    public SearchUsersQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CursorPage<UserSearchSummaryDto>>> Handle(
        SearchUsersQuery request,
        CancellationToken cancellationToken)
    {
        var queryText = request.Query.Trim();

        var cursor = ParseCursor(request.Cursor);
        if (request.Cursor is not null && cursor is null)
        {
            return Result.Fail<CursorPage<UserSearchSummaryDto>>(new Error(
                "Pagination.InvalidCursor",
                "Cursor must be a valid ISO 8601 timestamp."));
        }

        var query = _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                EF.Functions.ILike(user.UserName!, $"%{queryText}%") ||
                EF.Functions.ILike(user.DisplayName, $"%{queryText}%"));

        if (cursor.HasValue)
        {
            query = query.Where(user => user.CreatedAt < cursor.Value);
        }

        var rows = await query
            .OrderByDescending(user => user.CreatedAt)
            .Take(request.Limit + 1)
            .Select(user => new UserSearchRow(
                new UserSearchSummaryDto(
                    user.Id,
                    user.UserName!,
                    user.DisplayName,
                    user.AvatarUrl),
                user.CreatedAt))
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > request.Limit;
        var pageItems = rows.Take(request.Limit).ToList();
        var nextCursor = hasMore && pageItems.Count > 0
            ? pageItems[^1].CreatedAt.ToString("O")
            : null;

        return new CursorPage<UserSearchSummaryDto>(
            pageItems.Select(row => row.User).ToList(),
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

    private sealed record UserSearchRow(UserSearchSummaryDto User, DateTime CreatedAt);
}
