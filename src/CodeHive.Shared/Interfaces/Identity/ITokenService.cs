using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CodeHive.Shared.Interfaces.Identity;

public interface ITokenService
{
    AccessToken  GenerateAccessToken(Guid UserId , string email , IEnumerable<string>  roles);

    RefreshToken CreateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromToken(string accessToken);

}



public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);
public sealed record RefreshToken(string Value, DateTime ExpiresAtUtc);