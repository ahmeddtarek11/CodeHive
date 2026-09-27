using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CodeHive.Infrastructure.Identity;
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
using CodeHive.Posts.Application.Queries.GetBookmarks;
using CodeHive.Users.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication;
using CodeHive.Shared;
using Microsoft.Extensions.Options;

namespace CodeHive.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/v1/auth").WithTags("Auth").RequireRateLimiting("auth");
        auth.MapPost("/register", Register).AllowAnonymous()
            .WithSummary("Register a new user").WithDescription("Creates a new user account.");
        auth.MapPost("/login", Login).AllowAnonymous()
            .WithSummary("Login").WithDescription("Authenticates a user and returns a JWT token.");
        auth.MapPost("/refresh", Refresh).AllowAnonymous()
            .WithSummary("Refresh token").WithDescription("Refreshes an expired JWT token using a refresh token.");
        auth.MapGet("/login/google", GoogleLogin).AllowAnonymous()
            .WithSummary("Google Login").WithDescription("Initiates Google OAuth flow.");
        auth.MapGet("/oauth/google/callback", GoogleAuth).AllowAnonymous()
            .WithSummary("Google Callback").WithDescription("Handles Google OAuth callback.");
        auth.MapGet("/login/github", GitHubLogin).AllowAnonymous()
            .WithSummary("GitHub Login").WithDescription("Initiates GitHub OAuth flow.");
        auth.MapGet("/oauth/github/callback", GitHubAuth).AllowAnonymous()
            .WithSummary("GitHub Callback").WithDescription("Handles GitHub OAuth callback.");

        var users = app.MapGroup("/api/v1/users").WithTags("Users");
        users.MapGet("/me", GetCurrentProfile).RequireAuthorization()
            .WithSummary("Get the current user's profile")
            .WithDescription("Retrieves the complete profile for the authenticated user.");
        users.MapGet("/{username}", GetProfile).AllowAnonymous()
            .WithSummary("Get user profile").WithDescription("Retrieves the public profile of a user by username.");
        users.MapPost("/{id:guid}/follow", Follow).RequireAuthorization()
            .WithSummary("Follow a user").WithDescription("Follows another user by their ID.");
        users.MapDelete("/{id:guid}/follow", Unfollow).RequireAuthorization()
            .WithSummary("Unfollow a user").WithDescription("Unfollows a previously followed user.");
        users.MapGet("/{id:guid}/followers", GetFollowers).AllowAnonymous()
            .WithSummary("Get followers").WithDescription("Retrieves a paginated list of followers for a user.");
        users.MapGet("/{id:guid}/following", GetFollowing).AllowAnonymous()
            .WithSummary("Get following").WithDescription("Retrieves a paginated list of users that a user follows.");
        users.MapPut("/me", UpdateProfile).RequireAuthorization()
            .WithSummary("Update profile").WithDescription("Updates the profile of the currently authenticated user.");
        users.MapGet("/me/bookmarks", GetBookmarks).RequireAuthorization()
            .WithSummary("Get bookmarks").WithDescription("Retrieves a paginated list of bookmarked posts for the current user.");

        return app;
    }

    private static IResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/v1/auth/oauth/google/callback"
        };

        return Results.Challenge(properties, ["Google"]);
    }

    private static IResult GitHubLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/v1/auth/oauth/github/callback"
        };

        return Results.Challenge(properties, ["GitHub"]);
    }

    private static Task<IResult> GoogleAuth(
        HttpContext context,
        IExternalAuthService externalAuth,
        IOptions<OAuthSettings> oauthSettings)
    {
        return HandleExternalAuthCallback(
            context,
            externalAuth,
            oauthSettings,
            provider: "Google",
            avatarClaimType: "urn:google:picture");
    }

    private static Task<IResult> GitHubAuth(
        HttpContext context,
        IExternalAuthService externalAuth,
        IOptions<OAuthSettings> oauthSettings)
    {
        return HandleExternalAuthCallback(
            context,
            externalAuth,
            oauthSettings,
            provider: "GitHub",
            avatarClaimType: "urn:github:avatar_url");
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

    private static async Task<IResult> GetCurrentProfile(
        ClaimsPrincipal user,
        UserManager<ApplicationUser> userManager,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var currentUser = await userManager.FindByIdAsync(userId.ToString());
        if (string.IsNullOrWhiteSpace(currentUser?.UserName))
        {
            return Results.NotFound();
        }

        return await GetProfile(currentUser.UserName, mediator);
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

    private static async Task<IResult> GetBookmarks(
        ClaimsPrincipal user,
        IMediator mediator,
        string? cursor = null,
        int limit = 20)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new GetBookmarksQuery(userId, cursor, limit));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Bookmarks fetched successfully", result.Value));
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdValue =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out userId);
    }


    private static async Task<IResult> HandleExternalAuthCallback(
        HttpContext context,
        IExternalAuthService externalAuth,
        IOptions<OAuthSettings> oauthSettings,
        string provider,
        string avatarClaimType)
    {
        try
        {
            var authResult = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!authResult.Succeeded || authResult.Principal is null)
            {
                return Results.BadRequest("OAuth authentication failed.");
            }

            var principal = authResult.Principal;
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? email;
            var avatarUrl = principal.FindFirstValue(avatarClaimType);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerKey))
            {
                return Results.BadRequest("OAuth authentication failed.");
            }

            var result = await externalAuth.HandleExternalLoginAsync(
                provider,
                providerKey,
                email,
                displayName ?? email,
                avatarUrl,
                context.RequestAborted);

            if (result.IsFailure)
            {
                return result.ToApiResult();
            }

            var dto = result.Value;
            var redirectUrl = $"{oauthSettings.Value.FrontendCallbackUrl}" +
                              $"?access_token={Uri.EscapeDataString(dto.AccessToken)}" +
                              $"&refresh_token={Uri.EscapeDataString(dto.RefreshToken)}";

            return Results.Redirect(redirectUrl);
        }
        finally
        {
            await context.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }













}
