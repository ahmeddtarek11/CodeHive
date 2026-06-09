using CodeHive.Shared;

namespace CodeHive.Notifications.Application;

public class NotificationErrors
{
    
    public static readonly Error NotFound = Error.NotFound("Notification");
    public static readonly Error NotOwner = Error.Forbidden();
}
