using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.DeletePost;

public record DeletePostCommand(
    Guid Id,
    Guid RequestingUserId
) : ICommand;
