using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Users.Dtos;

public record UserSummaryDto(
    Guid    Id,
    string  Username,
    string  DisplayName,
    string? AvatarUrl
);