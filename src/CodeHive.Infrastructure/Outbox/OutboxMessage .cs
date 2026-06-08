using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Infrastructure.Outbox;

public sealed class OutboxMessage 
{
    public Guid Id { get; set; } = Guid.NewGuid();


    // The fully qualified type name of the domain event, e.g.:
    // "CodeHive.Posts.Domain.Events.PostLikedEvent"
    public string EventType { get; set; } = string.Empty;


    public string Payload  { get; set; }  = string.Empty;

    public DateTime CreatedAt    { get; init; } = DateTime.UtcNow;

     // Set to UtcNow when the OutboxProcessor successfully publishes this message.
    // null = not yet processed.
    public DateTime? ProcessedAt { get; set; }


    public string?  Error { get; set; }
}
