namespace CodeHive.Shared.Events;

public interface IMessageSentEvent
{
    Guid MessageId { get; }
    Guid ConversationId { get; }
    Guid SenderId { get; }
    string SenderUsername { get; }
    Guid RecipientId { get; }
    string ContentPreview { get; }
}
