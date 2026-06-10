using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Chat.Domain.Dtos;

public record MessageDto(
    Guid     Id,
    Guid     SenderId,
    string   SenderUsername,
    string   Content,
    bool     IsRead,
    DateTime SentAt          // maps to BaseEntity.CreatedAt
);