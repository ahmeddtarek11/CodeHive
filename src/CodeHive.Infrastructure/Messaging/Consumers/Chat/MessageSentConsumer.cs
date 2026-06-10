using System.Threading.Tasks;
using CodeHive.Shared.Events;
using CodeHive.Shared.Notifications;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CodeHive.Infrastructure.Messaging.Consumers.Chat;

public class MessageSentConsumer : IConsumer<IMessageSentEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MessageSentConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Consume(ConsumeContext<IMessageSentEvent> context)
    {
        var ev = context.Message;
        
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>(); 

        await mediator.Send(new CreateNotificationCommand(
            RecipientId:   ev.RecipientId,
            Type:          NotificationType.NewMessage,
            ActorId:       ev.SenderId,
            ActorUsername: ev.SenderUsername,
            RelatedPostId: null
        ));
    }
}
