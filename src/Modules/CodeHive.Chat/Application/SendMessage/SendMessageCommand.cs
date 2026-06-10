using CodeHive.Chat.Domain.Dtos;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Chat.Application.SendMessage;

public record SendMessageCommand(
    Guid   ConversationId,
    Guid   SenderId,
    string Content
) : ICommand<MessageDto>; 
