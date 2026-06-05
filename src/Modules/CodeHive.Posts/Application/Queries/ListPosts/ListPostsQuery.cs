using CodeHive.Posts.Domain.Dtos;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Queries.ListPosts;

public sealed record ListPostsQuery(
    Guid? AuthorId = null,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<PostSummaryDto>>;
