using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;
using CodeHive.Infrastructure.Outbox;
using System.Text.Json;
using CodeHive.Posts.PostsEvents;

namespace CodeHive.Posts.Application.Commands.LikePost;

public sealed class LikePostCommandHandler : ICommandHandler<LikePostCommand>
{
    private readonly CodeHiveDbContext _dbContext;

    public LikePostCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _dbContext.Set<Post>()
            .FirstOrDefaultAsync(post => post.Id == request.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Fail(PostErrors.NotFound);
        }

        var alreadyLiked = await _dbContext.Set<PostLike>()
            .AnyAsync(postLike => postLike.PostId == request.PostId && postLike.UserId == request.UserId, cancellationToken);

        if (alreadyLiked)
        {
            return Result.Fail(PostErrors.AlreadyLiked);
        }

        _dbContext.Set<PostLike>().Add(new PostLike
        {
            PostId = request.PostId,
            UserId = request.UserId
        });

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        var msg = new PostLikedEvent(post.Id, post.AuthorId, request.UserId, user?.UserName ?? string.Empty);
        _dbContext.Set<OutboxMessage>().Add(new OutboxMessage
        {
            EventType = typeof(PostLikedEvent).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(msg)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
