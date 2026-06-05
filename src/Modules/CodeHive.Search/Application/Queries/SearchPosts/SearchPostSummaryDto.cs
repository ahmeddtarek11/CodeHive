namespace CodeHive.Search.Application.Queries.SearchPosts;

public sealed record SearchPostSummaryDto(
    Guid Id,
    string AuthorUsername,
    string? AuthorAvatar,
    string? Title,
    string ContentPreview,
    string[] Tags,
    int LikeCount,
    int CommentCount,
    DateTime CreatedAt
);
