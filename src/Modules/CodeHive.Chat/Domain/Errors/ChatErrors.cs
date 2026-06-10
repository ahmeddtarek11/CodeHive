using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Chat.Domain.Dtos;
using CodeHive.Shared;

namespace CodeHive.Chat.Domain.Errors;

public static class ChatErrors
{
    public static readonly Error ConversationNotFound = Error.NotFound("Conversation");
    public static readonly Error MessageNotFound = Error.NotFound("Message");
    public static readonly Error NotParticipant= Error.Forbidden();
    public static readonly Error CannotMessageSelf =
        new("Chat.CannotMessageSelf", "You cannot start a conversation with yourself.");
    public static readonly Error ContentTooLong =
        new("Chat.ContentTooLong", "Message content cannot exceed 2000 characters.");

   
}