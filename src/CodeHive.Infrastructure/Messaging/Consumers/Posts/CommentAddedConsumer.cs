using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Events;
using CodeHive.Shared.Notifications;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CodeHive.Infrastructure.Messaging.Consumers.Posts;

public class CommentAddedConsumer : IConsumer<ICommentAddedEvent>
{
    private readonly IServiceScopeFactory _scopefactory;


    public CommentAddedConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopefactory = scopeFactory;    
    }
    public async Task Consume(ConsumeContext<ICommentAddedEvent> context)
    {
       var msg = context.Message;

        if(msg.AuthorId == msg.PostAuthorId) return ;

       using var scope = _scopefactory.CreateScope();
       var mediator =  scope.ServiceProvider.GetRequiredService<IMediator>();

       await mediator.Send(new CreateNotificationCommand(
            RecipientId:   msg.PostAuthorId,
            Type:          NotificationType.CommentAdded,
            ActorId:       msg.AuthorId,
            ActorUsername: msg.AuthorUserName,
            RelatedPostId: msg.PostId
        ));
    }
}
