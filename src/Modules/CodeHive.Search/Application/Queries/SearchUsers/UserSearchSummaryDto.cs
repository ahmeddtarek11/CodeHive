namespace CodeHive.Search.Application.Queries.SearchUsers;

public sealed record UserSearchSummaryDto(
    Guid Id,
    string UserName,
    string? DisplayName,
    string? AvatarUrl
);
