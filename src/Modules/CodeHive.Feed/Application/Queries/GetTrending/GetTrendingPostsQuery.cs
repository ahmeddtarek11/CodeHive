using CodeHive.Posts.Domain.Dtos;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Feed.Application.Queries.GetTrending;

public sealed record GetTrendingPostsQuery() : IQuery<List<PostSummaryDto>>;
