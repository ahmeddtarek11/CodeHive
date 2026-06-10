using CodeHive.Chat.Domain.Dtos;

namespace CodeHive.Chat.SignalR;

public interface IChatHubService
{
    

    Task SendMessageAsync(
        Guid recipientId,
        Guid conversationId,
        MessageDto message,
        CancellationToken ct = default);



    Task SendTypingIndicatorAsync(
        Guid recipientId,
        Guid conversationId,
        string senderUsername,
        CancellationToken ct = default);
}
