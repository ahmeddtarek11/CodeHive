using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CodeHive.Shared.Extensions;
using CodeHive.Shared.Responses;
using CodeHive.Users.Application.Commands.Follow;
using CodeHive.Users.Application.Commands.Login;
using CodeHive.Users.Application.Commands.Unfollow;
using CodeHive.Users.Application.Commands.RefreshToken;
using CodeHive.Users.Application.Commands.Register;
using CodeHive.Users.Application.Commands.UpdateProfile;
using CodeHive.Users.Application.Queries.GetFollowers;
using CodeHive.Users.Application.Queries.GetFollowing;
using CodeHive.Users.Application.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication;
using CodeHive.Shared;

namespace CodeHive.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/v1/auth").WithTags("Auth");
        auth.MapPost("/register", Register).AllowAnonymous();
        auth.MapPost("/login", Login).AllowAnonymous();
        auth.MapPost("/refresh", Refresh).AllowAnonymous();
        auth.MapGet("/login/google" ,GoogleLogin).AllowAnonymous();
        auth.MapGet("/oauth/google/callback" , GoogleAuth).AllowAnonymous();




        var users = app.MapGroup("/api/v1/users").WithTags("Users");
        users.MapGet("/{username}", GetProfile).AllowAnonymous();
        users.MapPost("/{id:guid}/follow", Follow).RequireAuthorization();
        users.MapDelete("/{id:guid}/follow", Unfollow).RequireAuthorization();
        users.MapGet("/{id:guid}/followers", GetFollowers).AllowAnonymous();
        users.MapGet("/{id:guid}/following", GetFollowing).AllowAnonymous();
        users.MapPut("/me", UpdateProfile).RequireAuthorization();

        return app;
    }

    private static async Task GoogleAuth(HttpContext context)
    {
        throw new NotImplementedException();
    }


    private static async Task<IResult> GoogleLogin(HttpContext context)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/v1/auth/oauth/google/callback"
        };

        return Results.Challenge(properties ,["Google"]);
    }


    private static async Task<IResult> Register(RegisterCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
            return result.ToApiResult();

        var username = command.Username.Trim().ToLowerInvariant();

        return Results.Created(
            $"/api/v1/users/{username}",
            ApiResponseFactory.Success(
                "User created successfully",
                new { userId = result.Value }));
    }

    private static async Task<IResult> Login(LoginCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success("Login successful", result.Value));
    }

    private static async Task<IResult> Refresh(RefreshTokenCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success("Token refreshed successfully", result.Value));
    }

    private static async Task<IResult> GetProfile(string username, IMediator mediator)
    {
        var result = await mediator.Send(new GetProfileQuery(username));

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success("Profile fetched successfully", result.Value));
    }

    private static async Task<IResult> UpdateProfile(
        UpdateProfileCommand command,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(command with { RequestingUserId = userId });

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success("Profile updated successfully", result.Value));
    }

    private static async Task<IResult> Follow(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var followerId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new FollowCommand(followerId, id));

        return result.ToApiResult();
    }

    private static async Task<IResult> Unfollow(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var followerId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new UnfollowCommand(followerId, id));

        return result.ToApiResult();
    }

    private static async Task<IResult> GetFollowers(
        Guid id,
        IMediator mediator,
        string? cursor = null,
        int limit = 20)
    {
        var result = await mediator.Send(new GetFollowersQuery(id, cursor, limit));

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success("Followers fetched successfully", result.Value));
    }

    private static async Task<IResult> GetFollowing(
        Guid id,
        IMediator mediator,
        string? cursor = null,
        int limit = 20)
    {
        var result = await mediator.Send(new GetFollowingQuery(id, cursor, limit));

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success("Following fetched successfully", result.Value));
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdValue =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out userId);
    }
}
