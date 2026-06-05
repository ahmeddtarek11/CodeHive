using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Posts.Application.Commands.CreatePost;

public record CreatePostCommand(
    PostType? Type,
    string? Title,
    string Content,
    string? Language,
    string[]? Tags,
    Guid AuthorId
) : ICommand<Guid>;
