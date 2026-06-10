using System.Globalization;
using CodeHive.Chat.Domain.Dtos;
using CodeHive.Chat.Domain.Entities;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Chat.Application.Queries.GetConversations;

public class GetConversationsQueryHandler : IQueryHandler<GetConversationsQuery, CursorPage<ConversationDto>>
{
    private readonly CodeHiveDbContext _db;

    public GetConversationsQueryHandler(CodeHiveDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CursorPage<ConversationDto>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<Conversation>()
            .AsNoTracking()
            .Where(c => c.LowerUserId_init == request.UserId || c.HigherUserId == request.UserId);

        if (request.Cursor is not null)
        {
            var cursorDate = DateTime.Parse(request.Cursor, null, DateTimeStyles.RoundtripKind);
            query = query.Where(c => c.LastMessageAt < cursorDate);
        }

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt)
            .Take(request.Limit + 1)
            .Select(c => new
            {
                c.Id,
                c.LowerUserId_init,
                c.HigherUserId,
                c.LastMessageAt,
                LastMessagePreview = _db.Set<Message>()
                    .Where(m => m.ConversationId == c.Id && !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),
                UnreadCount = _db.Set<Message>().Count(m =>
                    m.ConversationId == c.Id &&
                    m.SenderId != request.UserId &&
                    !m.IsRead)
            })
            .ToListAsync(cancellationToken);

        var hasMore = conversations.Count > request.Limit;
        if (hasMore)
        {
            conversations.RemoveAt(conversations.Count - 1);
        }

        var nextCursor = hasMore ? conversations[^1].LastMessageAt.ToString("O") : null;

        var dtos = new List<ConversationDto>();
        foreach (var c in conversations)
        {
            dtos.Add(new ConversationDto(
                c.Id,
                null!, // we will replace it soon
                c.LastMessagePreview,
                c.LastMessageAt,
                c.UnreadCount
            ));
        }

        var userIds = conversations
            .Select(c => c.LowerUserId_init == request.UserId ? c.HigherUserId : c.LowerUserId_init)
            .Distinct()
            .ToList();
        
        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserSummaryDto(u.Id, u.UserName!, u.DisplayName ?? u.UserName!, u.AvatarUrl))
            .ToDictionaryAsync(u => u.Id, cancellationToken);
            
        for(int i = 0; i < dtos.Count; i++)
        {
            var otherParticipantId = conversations[i].LowerUserId_init == request.UserId ? conversations[i].HigherUserId : conversations[i].LowerUserId_init;
            if (users.TryGetValue(otherParticipantId, out var user))
            {
                dtos[i] = dtos[i] with { OtherParticipant = user };
            }
        }

        return new CursorPage<ConversationDto>(dtos, nextCursor, hasMore);
    }
}
