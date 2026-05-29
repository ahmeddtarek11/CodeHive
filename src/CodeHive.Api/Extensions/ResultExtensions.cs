using CodeHive.Shared;

namespace CodeHive.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult(this Result result)
        => Shared.Extensions.ResultExtensions.ToApiResult(result);

    public static IResult ToApiResult<T>(this Result<T> result)
        => Shared.Extensions.ResultExtensions.ToApiResult(result);
}
