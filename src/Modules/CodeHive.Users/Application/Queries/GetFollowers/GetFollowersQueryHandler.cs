using System.Globalization;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Domain.Data.Entities;
using CodeHive.Users.Domain.Errors;
using CodeHive.Users.Dtos;
using Microsoft.EntityFrameworkCore;
using FollowEntity = CodeHive.Users.Domain.Data.Entities.Follow;

namespace CodeHive.Users.Application.Queries.GetFollowers;

public sealed class GetFollowersQueryHandler : IQueryHandler<GetFollowersQuery, CursorPage<UserSummaryDto>>
{
    private readonly CodeHiveDbContext _dbContext;

    public GetFollowersQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CursorPage<UserSummaryDto>>> Handle(
        GetFollowersQuery request,
        CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return Result.Fail<CursorPage<UserSummaryDto>>(UserErrors.NotFound);
        }

        var cursor = ParseCursor(request.Cursor);
        if (request.Cursor is not null && cursor is null)
        {
            return Result.Fail<CursorPage<UserSummaryDto>>(new Error("Pagination.InvalidCursor", "Cursor must be a valid ISO 8601 timestamp."));
        }

        var rows = await (
            from follow in _dbContext.Set<FollowEntity>().AsNoTracking()
            join user in _dbContext.Users.AsNoTracking() on follow.FollowerId equals user.Id
            where follow.FolloweeId == request.UserId
                  && (!cursor.HasValue || follow.FollowedAt < cursor.Value)
            orderby follow.FollowedAt descending
            select new FollowerPageRow(
                new UserSummaryDto(
                    user.Id,
                    user.UserName!,
                    user.DisplayName,
                    user.AvatarUrl),
                follow.FollowedAt))
            .Take(request.Limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > request.Limit;
        var pageItems = rows.Take(request.Limit).ToList();
        var nextCursor = hasMore && pageItems.Count > 0
            ? pageItems[^1].FollowedAt.ToString("O")
            : null;

        return new CursorPage<UserSummaryDto>(
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

    private sealed record FollowerPageRow(UserSummaryDto User, DateTime FollowedAt);
}
