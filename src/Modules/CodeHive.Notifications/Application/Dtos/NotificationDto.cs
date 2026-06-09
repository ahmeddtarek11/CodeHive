using CodeHive.Shared.Notifications;

namespace CodeHive.Notifications.Application.Dtos;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string ActorUsername,
    Guid? RelatedPostId,
    bool IsRead,
    DateTime  CreatedAt
);
