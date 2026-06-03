using CodeHive.Shared;
using CodeHive.Users.Dtos;

namespace CodeHive.Users.Domain;

public interface IExternalAuthService
{
    
    Task<Result<AuthTokensDto>> HandleExternalLoginAsync(string LoginProvider , 
    string ProviderKey , string Email, string DisplayName , 
    string? AvatarUrl , CancellationToken ct = default);




}
