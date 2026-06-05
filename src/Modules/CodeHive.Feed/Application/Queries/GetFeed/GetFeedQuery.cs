using CodeHive.Posts.Domain.Dtos;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Feed.Application.Queries.GetFeed;

public sealed record GetFeedQuery(
    Guid CurrentUserId,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<PostSummaryDto>>;
