using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Dtos;

namespace CodeHive.Users.Application.Queries.GetFollowing;

public sealed record GetFollowingQuery(
    Guid UserId,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<UserSummaryDto>>;
