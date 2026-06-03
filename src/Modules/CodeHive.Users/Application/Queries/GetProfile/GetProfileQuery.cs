using CodeHive.Shared.Cqrs;
using CodeHive.Users.Dtos;

namespace CodeHive.Users.Application.Queries.GetProfile;

public record GetProfileQuery(string Username) : IQuery<UserProfileDto>;
