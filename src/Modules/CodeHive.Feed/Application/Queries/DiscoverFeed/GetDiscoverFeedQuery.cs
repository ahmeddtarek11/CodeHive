using CodeHive.Posts.Domain.Dtos;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Feed.Application.Queries.DiscoverFeed;

public sealed record GetDiscoverFeedQuery(
    Guid? CurrentUserId = null,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<PostSummaryDto>>;
