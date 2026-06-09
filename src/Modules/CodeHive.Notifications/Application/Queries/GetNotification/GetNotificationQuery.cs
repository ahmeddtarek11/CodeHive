using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Notifications.Application.Dtos;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Notifications.Application.Queries;

public record GetNotificationsQuery(
    Guid    CurrentUserId,
    string? Cursor,
    int     Limit = 20
) : IQuery<CursorPage<NotificationDto>>;
