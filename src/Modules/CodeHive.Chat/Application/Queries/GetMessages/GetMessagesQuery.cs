using CodeHive.Chat.Domain.Dtos;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Chat.Application.Queries.GetMessages;

public record GetMessagesQuery(
    Guid    ConversationId,
    Guid    RequestingUserId,
    string? Cursor,
    int     Limit = 30
) : IQuery<CursorPage<MessageDto>>;
