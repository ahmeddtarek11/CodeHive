using CodeHive.Shared.Events;
using MediatR;

namespace CodeHive.Chat.Application.Events;

public record MessageSentEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderId,
    string SenderUsername,
    Guid RecipientId,
    string ContentPreview
) : INotification, IMessageSentEvent;
