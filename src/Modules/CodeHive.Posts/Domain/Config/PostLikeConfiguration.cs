using CodeHive.Infrastructure.Identity;
using CodeHive.Posts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeHive.Posts.Domain.Config;

public sealed class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.HasKey(pl => new { pl.UserId, pl.PostId });

        builder.HasOne<Post>()
            .WithMany()
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pl => pl.PostId);
    }
}
