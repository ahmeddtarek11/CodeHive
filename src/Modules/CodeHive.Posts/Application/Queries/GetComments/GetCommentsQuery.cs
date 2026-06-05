using CodeHive.Posts.Domain.Dtos;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Queries.GetComments;

public sealed record GetCommentsQuery(
    Guid PostId,
    string? Cursor = null,
    int Limit = 20
) : IQuery<CursorPage<CommentDto>>;
