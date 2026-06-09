using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = CodeHive.Notification.Domain.Entities.Notification;

namespace CodeHive.Notifications.Application.Commands.MarkRead;

public sealed class MarkAllNotificationsReadCommandHandler : ICommandHandler<MarkAllNotificationsReadCommand>
{
    private readonly CodeHiveDbContext _db;

    public MarkAllNotificationsReadCommandHandler(CodeHiveDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        // Single UPDATE statement — no entity loading, no per-row round-trips
        await _db.Set<NotificationEntity>()
            .Where(n => n.RecipientId == request.CurrentUserId && !n.IsRead)
            .ExecuteUpdateAsync(
                s => s.SetProperty(n => n.IsRead, true),
                cancellationToken);

        return Result.Ok();
    }
}
