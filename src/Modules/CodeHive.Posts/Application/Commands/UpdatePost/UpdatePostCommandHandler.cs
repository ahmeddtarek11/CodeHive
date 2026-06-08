using CodeHive.Infrastructure.Caching;
using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.UpdatePost;

public sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand>
{
    private readonly CodeHiveDbContext _dbContext;
    private readonly ICacheService _cache;

    public UpdatePostCommandHandler(CodeHiveDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<Result> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _dbContext.Set<Post>()
            .FirstOrDefaultAsync(currentPost => currentPost.Id == request.Id, cancellationToken);

        if (post is null)
        {
            return Result.Fail(PostErrors.NotFound);
        }

        if (post.AuthorId != request.RequestingUserId)
        {
            return Result.Fail(PostErrors.NotAuthor);
        }

        post.Title = NormalizeOptionalText(request.Title);
        post.Content = request.Content;
        post.Language = NormalizeOptionalText(request.Language);
        post.UpdatedAt = DateTime.UtcNow;

        var existingPostTags = await _dbContext.Set<PostTag>()
            .Where(postTag => postTag.PostId == post.Id)
            .ToListAsync(cancellationToken);

        if (existingPostTags.Count > 0)
        {
            _dbContext.Set<PostTag>().RemoveRange(existingPostTags);
        }

        var normalizedTags = (request.Tags ?? Array.Empty<string>())
            .Select(NormalizeTag)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedTags.Length > 0)
        {
            var postTags = new List<PostTag>(normalizedTags.Length);

            foreach (var tagName in normalizedTags)
            {
                var tag = await _dbContext.Set<Tag>()
                    .FirstOrDefaultAsync(currentTag => currentTag.Name == tagName, cancellationToken);

                if (tag is null)
                {
                    tag = new Tag { Name = tagName };
                    _dbContext.Set<Tag>().Add(tag);
                }

                postTags.Add(new PostTag
                {
                    PostId = post.Id,
                    TagId = tag.Id
                });
            }

            _dbContext.Set<PostTag>().AddRange(postTags);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate cached feeds of all followers so they see the updated post content immediately
        // var followerIds = await _dbContext.Set<Follow>()
        //     .Where(f => f.FolloweeId == post.AuthorId)
        //     .Select(f => f.FollowerId)
        //     .ToListAsync(cancellationToken);


        // Direct query is used to guarantee immediate cache invalidation within the same transaction boundary
        // Event-driven approach is avoided here to prevent stale feed visibility caused by async propagation delays
        // invalidating data this way is valid in this stage , but when a user has 100,000 follower for example 
        // this becomes slow 
        // ------- to be implemented fanout problem background job in the next stage -------- // 

                var followerIds = await _dbContext.Database
            .SqlQuery<Guid>(
                $"""
                SELECT "FollowerId"
                FROM "Follow"
                WHERE "FolloweeId" = {request.RequestingUserId}
                """)
            .ToListAsync(cancellationToken);

        foreach (var followerId in followerIds)
            await _cache.RemoveByPrefixAsync(CacheKeys.FeedPrefix(followerId), cancellationToken);

        return Result.Ok();
    }

    private static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeTag(string tag)
        => tag.Trim().ToLowerInvariant();
}
