using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared.Events;

public interface ICommentAddedEvent
{
    Guid AuthorId{get;}
    string AuthorUserName {get;}
    Guid PostId {get;}
    Guid PostAuthorId {get;}
    Guid? ParentCommentId{get;}
    
}
