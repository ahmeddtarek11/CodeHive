using CodeHive.Chat.Domain.Dtos;
using CodeHive.Chat.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace CodeHive.Chat.SignalR;

public class ChatHubService : IChatHubService
{

    private readonly IHubContext<ChatHub> _hub;

    public ChatHubService(IHubContext<ChatHub> hub) => _hub = hub;

    public async Task SendMessageAsync(Guid recipientId, Guid conversationId, MessageDto message, CancellationToken ct = default)
    {
        await _hub.Clients.User(recipientId.ToString()).SendAsync("NewMessage" , new {conversationId , message} , ct);
    }

    public async Task SendTypingIndicatorAsync(Guid recipientId, Guid conversationId, string senderUsername, CancellationToken ct = default)
    {
        await _hub.Clients.User(recipientId.ToString())
            .SendAsync("UserTyping", new { conversationId, senderUsername }, ct);
    }

}
