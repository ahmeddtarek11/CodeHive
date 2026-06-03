using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Dtos;

namespace CodeHive.Users.Application.Queries.GetFollowers;

public sealed record GetFollowersQuery(
    Guid UserId,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<UserSummaryDto>>;
