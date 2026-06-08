using CodeHive.Shared;
using CodeHive.Shared.Notifications;

namespace CodeHive.Notification.Domain.Entities;

public class Notification :BaseEntity
{
    
    public Guid RecipientId { get; set; }
    public NotificationType Type { get; set; }
    public Guid ActorId { get; set; }   
    public string ActorUsername { get; set; }   = string.Empty;
    public Guid? RealtedPostId { get; set; }    
    public bool IsRead { get; set; } = false;




}
