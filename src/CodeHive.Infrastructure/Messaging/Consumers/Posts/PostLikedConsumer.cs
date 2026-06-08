using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared.Events;
using CodeHive.Shared.Notifications;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;

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
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new CreateNotificationCommand(
            RecipientId:   msg.PostAuthorId,
            Type:          NotificationType.PostLiked,
            ActorId:       msg.LikedByUserId,
            ActorUsername: msg.LikedByUsername,
            RelatedPostId: msg.PostId
        ));
    }
}
