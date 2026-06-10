using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Chat.Domain.Dtos;

public record ConversationDto(
    Guid           Id,
    UserSummaryDto OtherParticipant, // the other user, not the caller
    string?        LastMessagePreview,
    DateTime       LastMessageAt,
    int            UnreadCount
);
