using CodeHive.Chat.Domain.Entities;
using CodeHive.Chat.Domain.Errors;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Chat.Application.MarkMessageRead;

public class MarkMessageReadCommandHandler : ICommandHandler<MarkMessagesReadCommand>
{
     private readonly CodeHiveDbContext _db ;
    public MarkMessageReadCommandHandler(CodeHiveDbContext dbContext)
    {
        _db = dbContext;
    }
    public async Task<Result> Handle(MarkMessagesReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _db.Set<Conversation>().FindAsync(new object[] {request.ConversationId} ,cancellationToken);
        if (conversation is null) return Result.Fail(ChatErrors.ConversationNotFound);

        var isParticipant = conversation.LowerUserId_init == request.ReaderId ||
                            conversation.HigherUserId == request.ReaderId;

        if (!isParticipant) return Result.Fail(ChatErrors.NotParticipant);

        await _db.Set<Message>()
        .Where(m => m.ConversationId == request.ConversationId
             && m.SenderId       != request.ReaderId
             && !m.IsRead)
        .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), cancellationToken);

        return Result.Ok();




    }

}
