using CodeHive.Chat.Domain.Dtos;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Chat.Application.Queries.GetConversations;

public record GetConversationsQuery(Guid UserId, string? Cursor, int Limit = 20)
    : IQuery<CursorPage<ConversationDto>>;
