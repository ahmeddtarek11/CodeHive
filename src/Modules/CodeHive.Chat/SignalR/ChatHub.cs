using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CodeHive.Chat.SignalR;


[Authorize]
public class ChatHub : Hub
{
    
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId , $"chat:{conversationId}");
    }

     public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat:{conversationId}");
    }

     public async Task SendTyping(string conversationId, string senderUsername)
    {
        // Push to everyone in the group EXCEPT the sender
        await Clients.OthersInGroup($"chat:{conversationId}")
            .SendAsync("UserTyping", new { conversationId, senderUsername });
    }
}
