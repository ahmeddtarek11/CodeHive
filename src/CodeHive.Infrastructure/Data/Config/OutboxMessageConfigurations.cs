using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeHive.Infrastructure.Data.Config;

public class OutboxMessageConfigurations : IEntityTypeConfiguration<OutboxMessage>
{
     public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.EventType).IsRequired().HasMaxLength(500);
        builder.Property(o => o.Payload).IsRequired();
        // Index on ProcessedAt = null so the processor can efficiently find unprocessed rows
        builder.HasIndex(o => o.ProcessedAt);
    }

    
}
