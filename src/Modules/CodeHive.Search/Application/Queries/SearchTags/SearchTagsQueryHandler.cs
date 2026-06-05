using CodeHive.Infrastructure.Data;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace CodeHive.Search.Application.Queries.SearchTags;

public sealed class SearchTagsQueryHandler : IQueryHandler<SearchTagsQuery, List<string>>
{
    private readonly CodeHiveDbContext _dbContext;

    public SearchTagsQueryHandler(CodeHiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<string>>> Handle(
        SearchTagsQuery request,
        CancellationToken cancellationToken)
    {
        var queryText = request.Query.Trim();

        var tags = await _dbContext.Set<Tag>()
            .AsNoTracking()
            .Where(tag => EF.Functions.ILike(tag.Name, $"{queryText}%"))
            .OrderBy(tag => tag.Name)
            .Select(tag => tag.Name)
            .ToListAsync(cancellationToken);

        return tags;
    }
}
