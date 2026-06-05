using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Dtos;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Posts.Domain.Errors;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Posts.Application.Queries.GetPost;

public sealed class GetPostQueryHandler : IQueryHandler<GetPostQuery, PostDetailDto>
{
    private readonly CodeHiveDbContext _dbContext;

    public GetPostQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PostDetailDto>> Handle(GetPostQuery request, CancellationToken cancellationToken)
    {
        var dto = await (
            from post in _dbContext.Set<Post>().AsNoTracking()
            join author in _dbContext.Users.AsNoTracking() on post.AuthorId equals author.Id
            where post.Id == request.Id
            select new PostDetailDto(
                post.Id,
                new PostAuthorDto(
                    author.Id,
                    author.UserName!,
                    author.DisplayName,
                    author.AvatarUrl),
                post.Title,
                post.Content,
                post.Language,
                _dbContext.Set<PostTag>().AsNoTracking()
                    .Where(postTag => postTag.PostId == post.Id)
                    .Join(
                        _dbContext.Set<Tag>().AsNoTracking(),
                        postTag => postTag.TagId,
                        tag => tag.Id,
                        (_, tag) => tag.Name)
                    .OrderBy(tagName => tagName)
                    .ToArray(),
                _dbContext.Set<PostLike>().AsNoTracking().Count(postLike => postLike.PostId == post.Id),
                _dbContext.Set<Comment>().AsNoTracking().Count(comment => comment.PostId == post.Id),
                request.CurrentUserId.HasValue &&
                _dbContext.Set<PostLike>().AsNoTracking()
                    .Any(postLike => postLike.PostId == post.Id && postLike.UserId == request.CurrentUserId.Value),
                request.CurrentUserId.HasValue &&
                _dbContext.Set<Bookmark>().AsNoTracking()
                    .Any(bookmark => bookmark.PostId == post.Id && bookmark.UserId == request.CurrentUserId.Value),
                post.CreatedAt,
                post.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return Result.Fail<PostDetailDto>(PostErrors.NotFound);
        }

        return dto;
    }
}
