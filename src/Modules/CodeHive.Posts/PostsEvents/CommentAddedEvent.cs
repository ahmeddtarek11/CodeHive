using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Events;
using MediatR;

namespace CodeHive.Posts.PostsEvents;

public record CommentAddedEvent
        (Guid AuthorId,string AuthorUserName ,Guid PostId, Guid PostAuthorId ,Guid? ParentCommentId) : INotification, ICommentAddedEvent
;