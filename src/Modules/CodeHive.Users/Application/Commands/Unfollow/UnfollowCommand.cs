using CodeHive.Shared.Cqrs;

namespace CodeHive.Users.Application.Commands.Unfollow;

public sealed record UnfollowCommand(Guid FollowerId, Guid FolloweeId) : ICommand;
