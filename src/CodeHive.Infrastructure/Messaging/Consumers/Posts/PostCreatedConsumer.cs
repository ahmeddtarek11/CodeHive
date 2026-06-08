using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared.Events;
using CodeHive.Shared.Notifications;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

             foreach (var followerId in followerIds)
        {
            await mediator.Send(new CreateNotificationCommand(
                RecipientId:  followerId,
                Type:         NotificationType.NewPost,
                ActorId:      msg.AuthorId,
                ActorUsername: msg.AuthorUsername,
                RelatedPostId: msg.PostId
            ), context.CancellationToken);
        }


    }
}
