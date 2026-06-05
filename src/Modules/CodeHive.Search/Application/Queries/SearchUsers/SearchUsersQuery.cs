using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Search.Application.Queries.SearchUsers;

public sealed record SearchUsersQuery(
    string Query,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<UserSearchSummaryDto>>;
