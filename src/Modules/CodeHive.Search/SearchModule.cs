using CodeHive.Search.Application.Queries.SearchPosts;
using CodeHive.Search.Application.Queries.SearchTags;
using CodeHive.Search.Application.Queries.SearchUsers;
using CodeHive.Shared.Extensions;
using CodeHive.Shared.Responses;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CodeHive.Search;

public static class SearchModule
{
    public static IServiceCollection AddSearchModules(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SearchModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(SearchModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var search = app.MapGroup("/api/v1/search").WithTags("Search");

        search.MapGet("/posts", SearchPosts).AllowAnonymous()
            .WithSummary("Search posts").WithDescription("Searches for posts based on a query string.");
        search.MapGet("/users", SearchUsers).AllowAnonymous()
            .WithSummary("Search users").WithDescription("Searches for users based on a query string.");
        search.MapGet("/tags", SearchTags).AllowAnonymous()
            .WithSummary("Search tags").WithDescription("Searches for tags based on a query string.");

        return app;
    }

    private static async Task<IResult> SearchPosts(
        IMediator mediator,
        string q,
        string? cursor = null,
        int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest("Query is required.");
        }

        var result = await mediator.Send(new SearchPostsQuery(q, cursor, limit));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Posts searched successfully", result.Value));
    }

    private static async Task<IResult> SearchUsers(
        IMediator mediator,
        string q,
        string? cursor = null,
        int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest("Query is required.");
        }

        var result = await mediator.Send(new SearchUsersQuery(q, cursor, limit));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Users searched successfully", result.Value));
    }

    private static async Task<IResult> SearchTags(IMediator mediator, string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest("Query is required.");
        }

        var result = await mediator.Send(new SearchTagsQuery(q));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Tags searched successfully", result.Value));
    }
}
