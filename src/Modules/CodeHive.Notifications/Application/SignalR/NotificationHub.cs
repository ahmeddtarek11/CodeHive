using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Notifications.Application.Queries.GetUnreadCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

// namespace CodeHive.Infrastructure.SignalR;
namespace CodeHive.Notifications.Application.SignalR;


[Authorize]
public sealed class NotificationHub : Hub
{
    //server side methods that client can call can go here 
    // For now it is empty  the server only pushes, clients only listen

    private readonly IServiceScopeFactory _scopeFactory ;
    public NotificationHub(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    public override async Task OnConnectedAsync()
    {
        // invoked when a client connects , can be used for logging or tracking

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var userId = Guid.Parse(Context.UserIdentifier!);

        var result =  await mediator.Send(new GetUnreadCountQuery(userId));

        if (result.IsSuccess)
            await Clients.Caller.SendAsync("UnreadCount", result.Value);
        
       await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception ? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    
}
