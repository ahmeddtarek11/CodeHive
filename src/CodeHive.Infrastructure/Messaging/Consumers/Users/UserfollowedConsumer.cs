using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Events;
using CodeHive.Shared.Notifications;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace CodeHive.Infrastructure.Messaging.Consumers.Users;

public class UserfollowedConsumer : IConsumer<IUserFollowedEvent>
{

    private readonly IServiceScopeFactory _scopeFactory;

    public UserfollowedConsumer(IServiceScopeFactory scopeFactory)
    {
        
        _scopeFactory = scopeFactory;
    }


    public async Task Consume(ConsumeContext<IUserFollowedEvent> context)
    {
        
        var msg = context.Message;
        using var scope = _scopeFactory.CreateScope();
        var mediator =  scope.ServiceProvider.GetRequiredService<IMediator>(); 


        await mediator.Send(new CreateNotificationCommand (
            RecipientId:   msg.FolloweeId,
            Type:          NotificationType.NewFollower,
            ActorId:       msg.FollowerId,
            ActorUsername: msg.FollowerUsername,
            RelatedPostId: null
        ));




    }
}
