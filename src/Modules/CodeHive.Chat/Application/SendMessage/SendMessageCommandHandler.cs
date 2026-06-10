using CodeHive.Chat.Application.Events;
using CodeHive.Chat.Domain.Dtos;
using CodeHive.Chat.Domain.Entities;
using CodeHive.Chat.Domain.Errors;
using CodeHive.Chat.SignalR;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Chat.Application.SendMessage;

public class SendMessageCommandHandler : ICommandHandler<SendMessageCommand, MessageDto>
{

    private readonly CodeHiveDbContext _db ;
    private readonly IChatHubService _ChatHubservice;
    public SendMessageCommandHandler(CodeHiveDbContext dbContext ,IChatHubService ChatHubservice)
    {
        _db = dbContext;
        _ChatHubservice = ChatHubservice;
    }

    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _db.Set<Conversation>().FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation is null) return ChatErrors.ConversationNotFound;


        var isParticipant = conversation.HigherUserId   == request.SenderId
                     || conversation.LowerUserId_init == request.SenderId;


        if (!isParticipant) return ChatErrors.NotParticipant;

        var message = new Message
        {

        ConversationId = request.ConversationId,
        SenderId  = request.SenderId,
        Content = request.Content,

        };
            
            
        _db.Set<Message>().Add(message);

        conversation.LastMessageAt = DateTime.UtcNow;

        var senderUsername = await _db.Users.AsNoTracking()
            .Where(u => u.Id == request.SenderId)
            .Select(u => u.UserName!)
            .FirstAsync(cancellationToken);

        var dto = new MessageDto(
            message.Id,
            message.SenderId,
            senderUsername,
            message.Content,
            message.IsRead,
            message.CreatedAt);

        var recipientId =  conversation.LowerUserId_init == request.SenderId
                                    ? conversation.HigherUserId : conversation.LowerUserId_init;


            
        message.RasieDomainEvent(new MessageSentEvent(
                message.Id, 
                request.ConversationId, 
                request.SenderId, 
                senderUsername,
                recipientId, 
                request.Content // (or truncate it to a preview)
                ));

        await _db.SaveChangesAsync(cancellationToken);
        await _ChatHubservice.SendMessageAsync(recipientId, request.ConversationId, dto, cancellationToken);


            return dto;



    }

}
