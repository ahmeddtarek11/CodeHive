using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Notifications.Dtos;
using Microsoft.AspNetCore.SignalR;

// namespace CodeHive.Infrastructure.SignalR;
namespace CodeHive.Notifications.Application.SignalR;

public class NotificationHubService : INotificationHubService
{

    private readonly IHubContext<NotificationHub> _hub;


    public NotificationHubService(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }
    public async Task SendNotificationAsync(Guid recipientId, NotificationDto notification, CancellationToken ct = default)
    {

        // Clients.User resolves which WebSocket connections belong to this userId
        // using the NameIdentifier claim in the JWT
        
        await _hub.Clients.User(recipientId.ToString()).SendAsync("NewNotification" ,notification ,ct);
    }
}
