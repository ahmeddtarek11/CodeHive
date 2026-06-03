using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Users.Dtos;

public record UserProfileDto(
    Guid     Id,
    string   Username,
    string   DisplayName,
    string?  Bio,
    string?  AvatarUrl,
    int      FollowersCount,
    int      FollowingCount,
    DateTime JoinedAt
);
