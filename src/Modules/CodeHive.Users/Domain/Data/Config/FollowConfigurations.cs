using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Identity;
using CodeHive.Users.Domain.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeHive.Users.Domain.Data.Config;

public sealed class FollowConfigurations : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.HasKey(f => new { f.FollowerId, f.FolloweeId });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(f => f.FolloweeId)
            .OnDelete(DeleteBehavior.NoAction); // avoid multiple cascade paths

        builder.HasIndex(f => f.FolloweeId);
    }
}
