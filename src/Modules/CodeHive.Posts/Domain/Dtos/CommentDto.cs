namespace CodeHive.Posts.Domain.Dtos;

public record CommentDto(
    Guid Id,
    UserSummaryDto Author,
    string Content,
    DateTime CreatedAt,
    List<CommentDto> Replies
);
