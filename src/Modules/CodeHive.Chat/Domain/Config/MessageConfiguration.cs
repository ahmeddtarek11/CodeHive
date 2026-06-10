using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeHive.Chat.Domain.Config;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
       builder.HasKey(m => m.Id);
       builder.HasQueryFilter(m => !m.IsDeleted);

       builder.HasIndex(m => new { m.ConversationId, m.CreatedAt });

       builder.HasIndex(m => new { m.ConversationId, m.SenderId, m.IsRead });

        builder.Property(m => m.Content).IsRequired().HasMaxLength(5000);
        
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade); // deleting a conversation deletes its messages
    }
}
