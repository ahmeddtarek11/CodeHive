using CodeHive.Posts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeHive.Posts.Domain.Config;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasQueryFilter(comment => !comment.IsDeleted);

        builder.HasOne<Post>()
            .WithMany()
            .HasForeignKey(comment => comment.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(comment => comment.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(comment => new { comment.PostId, comment.CreatedAt });
    }
}
