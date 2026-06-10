using CodeHive.Shared.Cqrs;

namespace CodeHive.Chat.Application.MarkMessageRead;

public record MarkMessagesReadCommand(Guid ConversationId, Guid ReaderId) : ICommand;