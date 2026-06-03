using CodeHive.Shared.Cqrs;

namespace CodeHive.Users.Application.Commands.Follow;

public sealed record FollowCommand(Guid FollowerId, Guid FolloweeId) : ICommand;
