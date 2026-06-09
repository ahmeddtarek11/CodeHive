using CodeHive.Shared.Cqrs;

namespace CodeHive.Notifications.Application.Commands.MarkRead;

public sealed record MarkNotificationReadCommand(
    Guid NotificationId,
    Guid CurrentUserId
) : ICommand;
