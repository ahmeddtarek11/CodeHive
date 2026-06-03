using CodeHive.Shared;
using Microsoft.AspNetCore.Http;

namespace CodeHive.Shared.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult(this Result result)
        => result.IsSuccess
            ? Results.NoContent()
            : MapError(result.Error);

    public static IResult ToApiResult<T>(this Result<T> result)
        => result.IsSuccess
            ? Results.Ok(result.Value)
            : MapError(result.Error);

    private static IResult MapError(Error error)
        => error.Code switch
        {
            var c when c.EndsWith(".NotFound", StringComparison.Ordinal) => Results.NotFound(error),
            var c when c.EndsWith(".Forbidden", StringComparison.Ordinal) => Results.Forbid(),
            var c when c.EndsWith(".Unauthorized", StringComparison.Ordinal) => Results.Unauthorized(),
            var c when c.EndsWith(".InvalidCredentials", StringComparison.Ordinal) => Results.Unauthorized(),
            var c when c.EndsWith(".TokenInvalid", StringComparison.Ordinal) => Results.Unauthorized(),
            var c when c.EndsWith(".TokenExpired", StringComparison.Ordinal) => Results.Unauthorized(),
            var c when c.EndsWith(".Taken", StringComparison.Ordinal) => Results.Conflict(error),
            var c when c.EndsWith(".AlreadyExists", StringComparison.Ordinal) => Results.Conflict(error),
            var c when c.EndsWith(".AlreadyFollowing", StringComparison.Ordinal) => Results.Conflict(error),
            _ => Results.BadRequest(error)
        };
}
