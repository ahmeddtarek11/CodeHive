using CodeHive.Shared.Cqrs;

namespace CodeHive.Notifications.Application.Queries.GetUnreadCount;

public sealed record GetUnreadCountQuery(
    Guid CurrentUserId
) : IQuery<int>;
