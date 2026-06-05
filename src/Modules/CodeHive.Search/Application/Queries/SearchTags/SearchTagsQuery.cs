using CodeHive.Shared.Cqrs;

namespace CodeHive.Search.Application.Queries.SearchTags;

public sealed record SearchTagsQuery(string Query) : IQuery<List<string>>;
