using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared.Events;

public interface IUserFollowedEvent
{
     Guid   FolloweeId {get;}     // the user who was followed
    Guid   FollowerId {get;}
    string FollowerUsername {get;}
}
