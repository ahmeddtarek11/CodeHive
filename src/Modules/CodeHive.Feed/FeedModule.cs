using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CodeHive.Feed.Application.Queries.DiscoverFeed;
using CodeHive.Feed.Application.Queries.GetFeed;
using CodeHive.Feed.Application.Queries.GetTrending;
using CodeHive.Shared.Extensions;
using CodeHive.Shared.Responses;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CodeHive.Feed;

public static class FeedModule
{
    public static IServiceCollection AddFeedModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FeedModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(FeedModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var feed = app.MapGroup("/api/v1/feed").WithTags("Feed");
        feed.MapGet("", GetFeed).RequireAuthorization();
        feed.MapGet("/discover", GetDiscoverFeed).AllowAnonymous();
        feed.MapGet("/trending", GetTrendingPosts).AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetFeed(
        ClaimsPrincipal user,
        IMediator mediator,
        string? cursor = null,
        int limit = 20)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new GetFeedQuery(userId, cursor, limit));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Feed fetched successfully", result.Value));
    }

    private static async Task<IResult> GetTrendingPosts(IMediator mediator)
    {
        var result = await mediator.Send(new GetTrendingPostsQuery());

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Trending posts fetched successfully", result.Value));
    }

    private static async Task<IResult> GetDiscoverFeed(
        ClaimsPrincipal user,
        IMediator mediator,
        string? cursor = null,
        int limit = 20)
    {
        Guid? userId = TryGetUserId(user, out var parsedUserId)
            ? parsedUserId
            : null;

        var result = await mediator.Send(new GetDiscoverFeedQuery(userId, cursor, limit));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Discover feed fetched successfully", result.Value));
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdValue =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out userId);
    }
}
