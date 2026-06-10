using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Chat.Domain.Entities;
using CodeHive.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CodeHive.Chat.Domain.Config;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => new { c.HigherUserId, c.LowerUserId_init }).IsUnique();

         builder.HasIndex(c => c.LowerUserId_init);
         builder.HasIndex(c=>c.LowerUserId_init);
        // for GET all Conversation , a query to find userId in either ones 

         builder.HasIndex(c => c.LastMessageAt);  // For sorting conversation list by most recent activity


         builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.HigherUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.LowerUserId_init)
            .OnDelete(DeleteBehavior.NoAction); // avoid multiple cascade paths
    }
}
