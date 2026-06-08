using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared.Events;

public interface IPostCreatedEvent
{
     Guid   PostId {get;}
    Guid   AuthorId{get;}
    string AuthorUsername {get; }
}
