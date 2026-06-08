using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared.Events;

public interface IPostLikedEvent
{
    Guid PostId{get;}
    Guid PostAuthorId{get;}
    Guid LikedByUserId{get;}
    string LikedByUsername {get;}
}
