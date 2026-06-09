using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using MediatR;
using CodeHive.Notification.Domain.Entities;
using NotificationEntity = CodeHive.Notification.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;


namespace CodeHive.Notifications.Application.Commands.CreateNotification;

public class CreateNotificationCommandHandler : ICommand<Guid>
{


    private readonly CodeHiveDbContext _db;

    public CreateNotificationCommandHandler(CodeHiveDbContext db)
    {
        _db = db;
    }
    public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var existing = await _db.Set<NotificationEntity>().FirstOrDefaultAsync(n =>
        n.RecipientId == request.RecipientId && 
        n.Type == request.Type &&
        n.ActorId ==request.ActorId &&
        n.RealtedPostId == request.RealtedPostId ,cancellationToken);


        if(existing is not null ) return existing.Id;

    var notification = new NotificationEntity
    {
        RecipientId   = request.RecipientId,
        Type          = request.Type,
        ActorId       = request.ActorId,
        ActorUsername = request.ActorUsername,
        RealtedPostId = request.RealtedPostId,
    };


     _db.Set<NotificationEntity>().Add(notification);
    await _db.SaveChangesAsync(cancellationToken);

    // After saving, signal the real-time hub (added in Section 7)
    // await _hubService.SendNotificationAsync(cmd.RecipientId, notification.ToDto());

    return notification.Id;



    }
}
