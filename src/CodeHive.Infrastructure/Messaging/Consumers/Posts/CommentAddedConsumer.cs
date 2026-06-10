using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Events;
using CodeHive.Shared.Notifications;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        using var scope = _scopefactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CommentAddedConsumer>>();
        logger.LogInformation("CommentAddedConsumer received event for Post {PostId} by User {UserId}", msg.PostId, msg.AuthorId);

        if(msg.AuthorId == msg.PostAuthorId) 
        {
            logger.LogInformation("Skipping notification: Author commented on their own post.");
            return ;
        }

        var mediator =  scope.ServiceProvider.GetRequiredService<IMediator>();

        logger.LogInformation("CommentAddedConsumer sending CreateNotificationCommand for Recipient {RecipientId}", msg.PostAuthorId);
        
        await mediator.Send(new CreateNotificationCommand(
            RecipientId:   msg.PostAuthorId,
            Type:          NotificationType.CommentAdded,
            ActorId:       msg.AuthorId,
            ActorUsername: msg.AuthorUserName,
            RelatedPostId: msg.PostId
        ));
        
        logger.LogInformation("CommentAddedConsumer successfully sent CreateNotificationCommand");
    }
}
