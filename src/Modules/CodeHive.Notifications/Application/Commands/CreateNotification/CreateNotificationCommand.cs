
using CodeHive.Shared.Cqrs;
using CodeHive.Shared.Notifications;

namespace CodeHive.Notifications.Application.Commands.CreateNotification;

public record CreateNotificationCommand
(
 Guid RecipientId ,
 NotificationType Type ,
  Guid ActorId ,
  string ActorUsername,
  Guid? RealtedPostId
) : ICommand<Guid> ;