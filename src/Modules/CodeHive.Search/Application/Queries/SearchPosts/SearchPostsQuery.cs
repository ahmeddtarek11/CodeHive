using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Search.Application.Queries.SearchPosts;

public sealed record SearchPostsQuery(
    string Query,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<SearchPostSummaryDto>>;
