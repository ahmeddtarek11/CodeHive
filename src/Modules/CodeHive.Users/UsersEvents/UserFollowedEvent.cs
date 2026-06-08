using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Events;
using MediatR;

namespace CodeHive.Users.UsersEvents;

public record UserFollowedEvent(Guid FolloweeId, Guid FollowerId, string FollowerUsername)
    : INotification , IUserFollowedEvent;