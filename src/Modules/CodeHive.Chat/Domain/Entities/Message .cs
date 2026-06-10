using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared;

namespace CodeHive.Chat.Domain.Entities;

public sealed class Message :BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content{ get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
}
