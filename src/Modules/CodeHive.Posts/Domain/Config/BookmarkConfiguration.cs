using CodeHive.Infrastructure.Identity;
using CodeHive.Posts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeHive.Posts.Domain.Config;

public sealed class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.HasKey(b => new { b.UserId, b.PostId });

        builder.HasOne<Post>()
            .WithMany()
            .HasForeignKey(b => b.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.PostId);
    }
}
