using CodeHive.Infrastructure.Caching;
using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Feed.Application.Queries.GetTrending;

public sealed class GetTrendingPostsQueryHandler : IQueryHandler<GetTrendingPostsQuery, List<PostSummaryDto>>
{
    private readonly CodeHiveDbContext _dbContext;
     private readonly ICacheService _cache;

    public GetTrendingPostsQueryHandler(CodeHiveDbContext dbContext , ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<Result<List<PostSummaryDto>>> Handle(
        GetTrendingPostsQuery request,
        CancellationToken cancellationToken)
    {

        var cacheKey = CacheKeys.Trending();
        var cached = await _cache.GetAsync<List<PostSummaryDto>>(cacheKey , cancellationToken );
        if(cached is not null ) return cached ;

        var since = DateTime.UtcNow.AddHours(-24);

        var trendingPostIds = await (
            from like in _dbContext.Set<PostLike>().AsNoTracking()
            join post in _dbContext.Set<Post>().AsNoTracking() on like.PostId equals post.Id
            where like.LikedAt > since
            group post by new { post.Id, post.CreatedAt } into grouped
            orderby grouped.Count() descending, grouped.Key.CreatedAt descending
            select grouped.Key.Id)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (trendingPostIds.Count == 0)
        {
            return new List<PostSummaryDto>();
        }

        var rows = await (
            from post in _dbContext.Set<Post>().AsNoTracking()
            join author in _dbContext.Users.AsNoTracking() on post.AuthorId equals author.Id
            where trendingPostIds.Contains(post.Id)
            select new
            {
                Post = post,
                Author = author
            })
            .ToListAsync(cancellationToken);

        var byId = rows.ToDictionary(
            row => row.Post.Id,
            row => new PostSummaryDto(
                row.Post.Id,
                row.Author.UserName!,
                row.Author.AvatarUrl,
                row.Post.Title,
                row.Post.Content.Length > 200
                    ? row.Post.Content.Substring(0, 200)
                    : row.Post.Content,
                _dbContext.Set<PostTag>().AsNoTracking()
                    .Where(postTag => postTag.PostId == row.Post.Id)
                    .Join(
                        _dbContext.Set<Tag>().AsNoTracking(),
                        postTag => postTag.TagId,
                        tag => tag.Id,
                        (_, tag) => tag.Name)
                    .OrderBy(tagName => tagName)
                    .ToArray(),
                _dbContext.Set<PostLike>().AsNoTracking().Count(postLike => postLike.PostId == row.Post.Id),
                _dbContext.Set<Comment>().AsNoTracking().Count(comment => comment.PostId == row.Post.Id),
                row.Post.CreatedAt));

        var result = trendingPostIds
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .ToList();

            await _cache.SetAsync(cacheKey , result ,TimeSpan.FromMinutes(10));

        return result;
    }
}
