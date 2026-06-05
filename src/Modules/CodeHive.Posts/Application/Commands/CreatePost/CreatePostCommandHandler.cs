using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Commands.CreatePost;

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, Guid>
{
    private readonly CodeHiveDbContext _dbContext;

    public CreatePostCommandHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            AuthorId = request.AuthorId,
            Type = request.Type!.Value,
            Title = NormalizeOptionalText(request.Title),
            Content = request.Content,
            Language = NormalizeOptionalText(request.Language)
        };

        _dbContext.Set<Post>().Add(post);

        var normalizedTags = (request.Tags ?? Array.Empty<string>())
            .Select(NormalizeTag)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedTags.Length > 0)
        {
            var postTags = new List<PostTag>(normalizedTags.Length);

            foreach (var tagName in normalizedTags)
            {
                var tag = await _dbContext.Set<Tag>()
                    .FirstOrDefaultAsync(currentTag => currentTag.Name == tagName, cancellationToken);

                if (tag is null)
                {
                    tag = new Tag { Name = tagName };
                    _dbContext.Set<Tag>().Add(tag);
                }

                postTags.Add(new PostTag
                {
                    PostId = post.Id,
                    TagId = tag.Id
                });
            }

            _dbContext.Set<PostTag>().AddRange(postTags);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return post.Id;
    }

    private static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeTag(string tag)
        => tag.Trim().ToLowerInvariant();
}
