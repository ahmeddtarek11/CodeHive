using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = CodeHive.Notification.Domain.Entities.Notification;

namespace CodeHive.Notifications.Application.Queries.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler : IQueryHandler<GetUnreadCountQuery, int>
{
    private readonly CodeHiveDbContext _db;

    public GetUnreadCountQueryHandler(CodeHiveDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _db.Set<NotificationEntity>()
            .CountAsync(n => n.RecipientId == request.CurrentUserId && !n.IsRead, cancellationToken);

        return count;
    }
}
