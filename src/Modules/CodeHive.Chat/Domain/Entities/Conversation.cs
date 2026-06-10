using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Chat.Domain.Entities;

public sealed class Conversation
{
    public Guid Id  { get; init; } = Guid.NewGuid();
    public Guid LowerUserId_init   { get; set; }
    public Guid  HigherUserId { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Denormalized: updated every time a message is sent.
    // Used to sort conversation list by most recent activity — far cheaper than MAX(Message.SentAt).
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
}
