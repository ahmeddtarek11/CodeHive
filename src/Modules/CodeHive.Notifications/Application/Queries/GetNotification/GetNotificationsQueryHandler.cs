using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Shared.Notifications.Dtos;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = CodeHive.Notification.Domain.Entities.Notification;

namespace CodeHive.Notifications.Application.Queries;

public sealed class GetNotificationsQueryHandler
    : IQueryHandler<GetNotificationsQuery, CursorPage<NotificationDto>>
{
    private readonly CodeHiveDbContext _db;

    public GetNotificationsQueryHandler(CodeHiveDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CursorPage<NotificationDto>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        // Cursor format: "isRead|createdAt"  e.g. "False|2026-06-09T00:00:00.0000000Z"
        bool? cursorIsRead = null;
        DateTime? cursorDate = null;

        if (request.Cursor is not null)
        {
            var parts = request.Cursor.Split('|', 2);
            if (parts.Length != 2
                || !bool.TryParse(parts[0], out var parsedIsRead)
                || !DateTime.TryParse(parts[1], null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedDate))
            {
                return Result.Fail<CursorPage<NotificationDto>>(
                    new Error("Pagination.InvalidCursor", "Cursor is invalid."));
            }

            cursorIsRead = parsedIsRead;
            cursorDate   = parsedDate;
        }

        var limit = request.Limit;

        // Sort: unread (IsRead = false) first, then by CreatedAt DESC within each group.
        // The keyset predicate mirrors the compound sort key so we skip exactly the rows
        // already returned without using OFFSET:
        //   • moved into the "read" half:   IsRead = true AND cursor was on unread
        //   • same half, older item:         IsRead == cursorIsRead AND CreatedAt < cursorDate
        var baseQuery = _db.Set<NotificationEntity>()
            .Where(n => n.RecipientId == request.CurrentUserId);

        var filteredQuery = cursorDate is null
            ? baseQuery
            : baseQuery.Where(n =>
                (n.IsRead && !cursorIsRead!.Value)
                || (n.IsRead == cursorIsRead && n.CreatedAt < cursorDate));

        var items = await filteredQuery
            .OrderBy(n => n.IsRead)              // false (unread) before true (read)
            .ThenByDescending(n => n.CreatedAt)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        var dtos = items.Select(n => new NotificationDto(
            n.Id,
            n.Type,
            n.ActorUsername,
            n.RealtedPostId,
            n.IsRead,
            n.CreatedAt
        )).ToList();

        // Encode the last item's compound sort key as the next cursor
        var nextCursor = hasMore
            ? $"{items[^1].IsRead}|{items[^1].CreatedAt:O}"
            : null;

        return new CursorPage<NotificationDto>(dtos, nextCursor, hasMore);
    }
}
