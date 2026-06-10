using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared.Events;
using CodeHive.Shared.Notifications;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeHive.Infrastructure.Messaging.Consumers.Posts;

public class PostCreatedConsumer : IConsumer<IPostCreatedEvent>
{

    private readonly IServiceScopeFactory _scopeFactory;

    public PostCreatedConsumer(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;




    public async Task Consume(ConsumeContext<IPostCreatedEvent> context)
    {
        var msg = context.Message;
        using var scope = _scopeFactory.CreateScope();
        
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PostCreatedConsumer>>();
        logger.LogInformation("PostCreatedConsumer received event for Post {PostId} by User {UserId}", msg.PostId, msg.AuthorId);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<CodeHiveDbContext>();

        // Get all follower IDs for this author
        var followerIds = await db.Database
            .SqlQuery<Guid>(
                $"""
                SELECT "FollowerId"
                FROM "Follow"
                WHERE "FolloweeId" = {msg.AuthorId}
                """)
            .ToListAsync();

        logger.LogInformation("PostCreatedConsumer found {Count} followers to notify", followerIds.Count);

        foreach (var followerId in followerIds)
        {
            logger.LogInformation("PostCreatedConsumer sending CreateNotificationCommand for Follower {FollowerId}", followerId);
            await mediator.Send(new CreateNotificationCommand(
                RecipientId: followerId,
                Type: NotificationType.NewPost,
                ActorId: msg.AuthorId,
                ActorUsername: msg.AuthorUsername,
                RelatedPostId: msg.PostId
            ), context.CancellationToken);
        }
        
        logger.LogInformation("PostCreatedConsumer successfully processed {Count} notifications", followerIds.Count);
    }
}
