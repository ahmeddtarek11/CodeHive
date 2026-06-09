using CodeHive.Shared.Cqrs;

namespace CodeHive.Notifications.Application.Commands.MarkRead;

public sealed record MarkAllNotificationsReadCommand(
    Guid CurrentUserId
) : ICommand;
