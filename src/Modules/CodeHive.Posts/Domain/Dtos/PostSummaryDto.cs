namespace CodeHive.Posts.Domain.Dtos;

public record PostSummaryDto(
    Guid         Id,
    string       AuthorUsername,
    string?      AuthorAvatar,
    string?      Title,
    string       ContentPreview,  // first 200 chars
    string[]     Tags,
    int          LikeCount,
    int          CommentCount,
    DateTime     CreatedAt
);
