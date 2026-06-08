using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Events;
using MediatR;

namespace CodeHive.Posts.PostsEvents;

public record PostCreatedEvent(Guid PostId, Guid AuthorId, string AuthorUsername) : INotification , IPostCreatedEvent;