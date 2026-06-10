using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Shared.Notifications.Dtos;

namespace CodeHive.Notifications.Application.Queries;

public record GetNotificationsQuery(
    Guid    CurrentUserId,
    string? Cursor,
    int     Limit = 20
) : IQuery<CursorPage<NotificationDto>>;
