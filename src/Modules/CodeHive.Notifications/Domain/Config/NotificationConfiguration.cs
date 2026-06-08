using Microsoft.EntityFrameworkCore;

namespace CodeHive.Notifications.Domain.Config;
using CodeHive.Notification.Domain.Entities;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        // Index for the most common query: "all unread notifications for userId, newest first"
        builder.HasIndex(n => new { n.RecipientId, n.IsRead, n.CreatedAt });

        builder.Property(n => n.ActorUsername).IsRequired().HasMaxLength(100);
    }

}
