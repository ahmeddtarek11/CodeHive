using CodeHive.Chat.Domain.Entities;
using CodeHive.Chat.Domain.Errors;
using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Chat.Application.StartConversation;

public class StartConversationCommandHandler : ICommandHandler<StartConversationCommand, Guid>
{

    private readonly CodeHiveDbContext _db ;

    public StartConversationCommandHandler(CodeHiveDbContext dbContext)
    {
        _db = dbContext;        
    }
    public async Task<Result<Guid>> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        if(request.initatorUserId == request.targetUserId)
        {
            return ChatErrors.CannotMessageSelf;
        }

        var targetExists = await _db.Users.AnyAsync(u=>u.Id == request.targetUserId , cancellationToken);
        if(!targetExists) return Error.NotFound("User");

        var (smallerId , largerId) = request.initatorUserId < request.targetUserId ? 
                (request.initatorUserId ,request.targetUserId) : 
                (request.targetUserId ,request.initatorUserId );


        var existing  = await _db.Set<Conversation>().Where(c=>c.LowerUserId_init == smallerId 
                                                                    && c.HigherUserId == largerId)
                                                                    .Select(c=>c.Id)
                                                                    .FirstOrDefaultAsync(cancellationToken);



        if (existing != Guid.Empty) return existing;


        var conversation =  new Conversation
        {
            LowerUserId_init = smallerId ,
            HigherUserId =largerId
        };

        _db.Set<Conversation>().Add(conversation);
        await _db.SaveChangesAsync(cancellationToken);
        
        return conversation.Id;
    }

}
