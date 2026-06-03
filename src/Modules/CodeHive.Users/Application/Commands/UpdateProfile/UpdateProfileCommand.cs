using CodeHive.Shared.Cqrs;
using CodeHive.Users.Dtos;

namespace CodeHive.Users.Application.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid RequestingUserId,
    string DisplayName,
    string? Bio,
    string? AvatarUrl
) : ICommand<UserProfileDto>;
