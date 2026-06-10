using System.Globalization;
using CodeHive.Chat.Domain.Dtos;
using CodeHive.Chat.Domain.Entities;
using CodeHive.Chat.Domain.Errors;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Chat.Application.Queries.GetMessages;

public class GetMessagesQueryHandler : IQueryHandler<GetMessagesQuery, CursorPage<MessageDto>>
{
    private readonly CodeHiveDbContext _db;

    public GetMessagesQueryHandler(CodeHiveDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CursorPage<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _db.Set<Conversation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation is null) return ChatErrors.ConversationNotFound;

        var isParticipant = conversation.LowerUserId_init == request.RequestingUserId
                         || conversation.HigherUserId == request.RequestingUserId;

        if (!isParticipant) return ChatErrors.NotParticipant;

        var query = _db.Set<Message>()
            .AsNoTracking()
            .Where(m => m.ConversationId == request.ConversationId && !m.IsDeleted);

        if (request.Cursor is not null)
        {
            var cursorDate = DateTime.Parse(request.Cursor, null, DateTimeStyles.RoundtripKind);
            query = query.Where(m => m.CreatedAt < cursorDate);
        }

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(request.Limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = messages.Count > request.Limit;
        if (hasMore)
        {
            messages.RemoveAt(messages.Count - 1);
        }

        var nextCursor = hasMore ? messages[^1].CreatedAt.ToString("O") : null;

        var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
        var senders = await _db.Users
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName!, cancellationToken);

        var dtos = messages.Select(m => new MessageDto(
            m.Id,
            m.SenderId,
            senders.GetValueOrDefault(m.SenderId, "Unknown"),
            m.Content,
            m.IsRead,
            m.CreatedAt
        )).ToList();

        return new CursorPage<MessageDto>(dtos, nextCursor, hasMore);
    }
}
