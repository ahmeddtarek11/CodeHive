using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = CodeHive.Notification.Domain.Entities.Notification;

namespace CodeHive.Notifications.Application.Commands.MarkRead;

public sealed class MarkNotificationReadCommandHandler : ICommandHandler<MarkNotificationReadCommand>
{
    private readonly CodeHiveDbContext _db;

    public MarkNotificationReadCommandHandler(CodeHiveDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _db.Set<NotificationEntity>()
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Fail(NotificationErrors.NotFound);

        if (notification.RecipientId != request.CurrentUserId)
            return Result.Fail(NotificationErrors.NotOwner);

        if (notification.IsRead)
            return Result.Ok(); // already read — idempotent, no-op

        notification.IsRead = true;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
