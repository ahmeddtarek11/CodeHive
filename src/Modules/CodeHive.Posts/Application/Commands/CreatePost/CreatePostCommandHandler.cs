using CodeHive.Infrastructure.Caching;
using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.PostsEvents;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace CodeHive.Posts.Application.Commands.CreatePost;

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, Guid>
{
    private readonly CodeHiveDbContext _dbContext;
    private readonly ICacheService _cache;
    

    public CreatePostCommandHandler(CodeHiveDbContext dbContext, ICacheService cache )
    {
        _dbContext = dbContext;
        _cache = cache;
        
    }

    public async Task<Result<Guid>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            AuthorId = request.AuthorId,
            Type = request.Type!.Value,
            Title = NormalizeOptionalText(request.Title),
            Content = request.Content,
            Language = NormalizeOptionalText(request.Language)
        };

        _dbContext.Set<Post>().Add(post);

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

        var user = await _dbContext.Users.FirstOrDefaultAsync(u=> u.Id ==post.AuthorId);
        post.RasieDomainEvent(new PostCreatedEvent(post.Id , post.AuthorId , user!.UserName!));

        await _dbContext.SaveChangesAsync(cancellationToken);

       



        // Direct query is used to guarantee immediate cache invalidation within the same transaction boundary
        // Event-driven approach is avoided here to prevent stale feed visibility caused by async propagation delays
        // invalidating data this way is valid in this stage , but when a user has 100,000 followerS for example 
        // this becomes slow 
        // ------- to be implemented fanout problem background job in the next stage -------- // 

            var followerIds = await _dbContext.Database
            .SqlQuery<Guid>(
                $"""
                SELECT "FollowerId"
                FROM "Follow"
                WHERE "FolloweeId" = {request.AuthorId}
                """)
            .ToListAsync(cancellationToken);

         foreach (var followerId in followerIds)
            await _cache.RemoveByPrefixAsync(CacheKeys.FeedPrefix(followerId), cancellationToken);



        return post.Id;
    }

    private static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeTag(string tag)
        => tag.Trim().ToLowerInvariant();
}
