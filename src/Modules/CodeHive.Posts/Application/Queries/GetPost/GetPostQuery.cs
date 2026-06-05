using CodeHive.Posts.Domain.Dtos;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Queries.GetPost;

public sealed record GetPostQuery(
    Guid Id,
    Guid? CurrentUserId = null
) : IQuery<PostDetailDto>;
