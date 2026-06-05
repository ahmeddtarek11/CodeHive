namespace CodeHive.Posts.Domain.Dtos;

public record PostDetailDto(
    Guid           Id,
    PostAuthorDto  Author,
    string?        Title,
    string         Content,
    string?        Language,
    string[]       Tags,
    int            LikeCount,
    int            CommentCount,
    bool           IsLikedByCurrentUser,
    bool           IsBookmarkedByCurrentUser,
    DateTime       CreatedAt,
    DateTime       UpdatedAt
);
