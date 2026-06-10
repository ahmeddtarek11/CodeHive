using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared.Events;
using CodeHive.Shared.Notifications;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CodeHive.Infrastructure.Messaging.Consumers.Posts;

public class PostLikedConsumer : IConsumer<IPostLikedEvent>
{

    private readonly IServiceScopeFactory _scopeFactory;

    public PostLikedConsumer(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task Consume(ConsumeContext<IPostLikedEvent> context)
    {
        var msg = context.Message;

        if (msg.PostAuthorId == msg.LikedByUserId) return;


        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PostLikedConsumer>>();
        logger.LogInformation("PostLikedConsumer received event for Post {PostId} by User {UserId}", msg.PostId, msg.LikedByUserId);

        if (msg.PostAuthorId == msg.LikedByUserId)
        {
            logger.LogInformation("Skipping notification: Author liked their own post.");
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        logger.LogInformation("PostLikedConsumer sending CreateNotificationCommand for Recipient {RecipientId}", msg.PostAuthorId);

        await mediator.Send(new CreateNotificationCommand(
            RecipientId: msg.PostAuthorId,
            Type: NotificationType.PostLiked,
            ActorId: msg.LikedByUserId,
            ActorUsername: msg.LikedByUsername,
            RelatedPostId: msg.PostId
        ));

        logger.LogInformation("PostLikedConsumer successfully sent CreateNotificationCommand");
    }
}
