using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Notifications.Dtos;

// namespace CodeHive.Infrastructure.SignalR;
namespace CodeHive.Notifications.Application.SignalR;

public interface INotificationHubService
{
    Task SendNotificationAsync (Guid recipientId , NotificationDto notification , CancellationToken ct = default);
}
