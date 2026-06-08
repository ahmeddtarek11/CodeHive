using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Events;
using MediatR;

namespace CodeHive.Posts.PostsEvents;

public record PostLikedEvent(Guid PostId, Guid PostAuthorId, Guid LikedByUserId, string LikedByUsername) : INotification , IPostLikedEvent;

