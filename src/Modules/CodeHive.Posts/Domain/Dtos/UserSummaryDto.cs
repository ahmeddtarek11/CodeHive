namespace CodeHive.Posts.Domain.Dtos;

public record UserSummaryDto(
    Guid Id,
    string Username,
    string DisplayName,
    string? AvatarUrl
);
