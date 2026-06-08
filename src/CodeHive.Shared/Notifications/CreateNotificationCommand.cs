using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Shared.Notifications;

public record CreateNotificationCommand(
    Guid             RecipientId,
    NotificationType Type,
    Guid             ActorId,
    string           ActorUsername,
    Guid?            RelatedPostId
) : ICommand<Guid>;

