using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Users.Dtos;


public record AuthTokensDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry
);
