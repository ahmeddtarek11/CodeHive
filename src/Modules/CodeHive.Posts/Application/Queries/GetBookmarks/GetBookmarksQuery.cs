using CodeHive.Posts.Domain.Dtos;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Queries.GetBookmarks;

public sealed record GetBookmarksQuery(
    Guid UserId,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<PostSummaryDto>>;
