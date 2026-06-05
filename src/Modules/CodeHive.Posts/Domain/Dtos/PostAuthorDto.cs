namespace CodeHive.Posts.Domain.Dtos;

public record PostAuthorDto(
    Guid    Id,
    string  Username,
    string  DisplayName,
    string? AvatarUrl
);