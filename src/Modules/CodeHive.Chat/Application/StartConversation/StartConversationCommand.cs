using CodeHive.Shared.Cqrs;

namespace CodeHive.Chat.Application.StartConversation;

public record StartConversationCommand(Guid initatorUserId, Guid targetUserId) : ICommand<Guid>;