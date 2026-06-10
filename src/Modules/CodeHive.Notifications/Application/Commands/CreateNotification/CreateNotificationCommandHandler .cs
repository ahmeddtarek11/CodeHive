using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using MediatR;
using CodeHive.Notification.Domain.Entities;
using NotificationEntity = CodeHive.Notification.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using CodeHive.Shared.Notifications.Dtos;
using CodeHive.Notifications.Application.SignalR;
using CodeHive.Shared.Notifications;

namespace CodeHive.Notifications.Application.Commands.CreateNotification;

public class CreateNotificationCommandHandler : ICommandHandler<CreateNotificationCommand, Guid>
{
    private readonly CodeHiveDbContext _db;
    private readonly INotificationHubService _NotificationHubService;

    public CreateNotificationCommandHandler(CodeHiveDbContext db, INotificationHubService NotificationHubService)
    {
        _db = db;
        _NotificationHubService = NotificationHubService;
    }

    public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var logger = _NotificationHubService.GetType().Assembly.GetType("CodeHive.Notifications.Application.SignalR.NotificationHubService")?
            .GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .GetValue(_NotificationHubService) as Microsoft.Extensions.Logging.ILogger;
            
        // Fallback if we can't easily inject logger here without breaking constructor
        Console.WriteLine($"[CreateNotificationCommandHandler] Processing notification for Recipient: {request.RecipientId}, Type: {request.Type}");

        var existing = await _db.Set<NotificationEntity>().FirstOrDefaultAsync(n =>
            n.RecipientId == request.RecipientId && 
            n.Type == request.Type &&
            n.ActorId == request.ActorId &&
            n.RealtedPostId == request.RelatedPostId, cancellationToken);

        if(existing is not null) 
        {
            Console.WriteLine($"[CreateNotificationCommandHandler] Notification already exists with ID: {existing.Id}");
            return existing.Id;
        }

        var notification = new NotificationEntity
        {
            RecipientId   = request.RecipientId,
            Type          = request.Type,
            ActorId       = request.ActorId,
            ActorUsername = request.ActorUsername,
            RealtedPostId = request.RelatedPostId,
        };

        _db.Set<NotificationEntity>().Add(notification);
        await _db.SaveChangesAsync(cancellationToken);
        
        Console.WriteLine($"[CreateNotificationCommandHandler] Successfully saved notification with ID: {notification.Id}");

        var NotificationDto = new NotificationDto(
            notification.Id,
            notification.Type,
            notification.ActorUsername,
            notification.RealtedPostId,
            notification.IsRead,
            notification.CreatedAt
        );

        //fire and forget , if the user isn't connected it gets the notification with polling GET /notfications
        await _NotificationHubService.SendNotificationAsync(request.RecipientId, NotificationDto, cancellationToken);
        
        Console.WriteLine($"[CreateNotificationCommandHandler] Sent SignalR notification");

        return notification.Id;
    }
}
