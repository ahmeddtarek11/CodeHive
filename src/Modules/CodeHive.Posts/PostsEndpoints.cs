using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CodeHive.Posts.Application.Commands.AddComment;
using CodeHive.Posts.Application.Commands.BookmarkPost;
using CodeHive.Posts.Application.Commands.DeleteComment;
using CodeHive.Posts.Application.Commands.LikePost;
using CodeHive.Posts.Application.Commands.RemoveBookmark;
using CodeHive.Posts.Application.Commands.UnlikePost;
using CodeHive.Posts.Application.Commands.CreatePost;
using CodeHive.Posts.Application.Commands.DeletePost;
using CodeHive.Posts.Application.Commands.UpdatePost;
using CodeHive.Posts.Application.Queries.GetComments;
using CodeHive.Posts.Application.Queries.GetPost;
using CodeHive.Posts.Application.Queries.ListPosts;
using CodeHive.Shared;
using CodeHive.Shared.Extensions;
using CodeHive.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeHive.Posts;

public static class PostsEndpoints
{
    public static IEndpointRouteBuilder MapPostsEndpoints(this IEndpointRouteBuilder app)
    {
        var posts = app.MapGroup("/api/v1/posts")
            .WithTags("Posts");

        var comments = posts.MapGroup("/{postId:guid}/comments");

        posts.MapGet("", ListPosts).AllowAnonymous()
            .WithSummary("List posts").WithDescription("Retrieves a paginated list of posts, optionally filtered by author.");
        posts.MapGet("/{id:guid}", GetPost).AllowAnonymous()
            .WithSummary("Get post").WithDescription("Retrieves a specific post by its ID.");
        posts.MapPost("", CreatePost).RequireAuthorization()
            .WithSummary("Create post").WithDescription("Creates a new post.");
        posts.MapPut("/{id:guid}", UpdatePost).RequireAuthorization()
            .WithSummary("Update post").WithDescription("Updates an existing post.");
        posts.MapDelete("/{id:guid}", DeletePost).RequireAuthorization()
            .WithSummary("Delete post").WithDescription("Deletes a post.");
        posts.MapPost("/{id:guid}/like", LikePost).RequireAuthorization()
            .WithSummary("Like post").WithDescription("Adds a like to a post.");
        posts.MapDelete("/{id:guid}/like", UnlikePost).RequireAuthorization()
            .WithSummary("Unlike post").WithDescription("Removes a like from a post.");
        posts.MapPost("/{id:guid}/bookmark", BookmarkPost).RequireAuthorization()
            .WithSummary("Bookmark post").WithDescription("Bookmarks a post for the current user.");
        posts.MapDelete("/{id:guid}/bookmark", RemoveBookmark).RequireAuthorization()
            .WithSummary("Remove bookmark").WithDescription("Removes a post from the current user's bookmarks.");

        comments.MapGet("", GetComments).AllowAnonymous()
            .WithSummary("Get comments").WithDescription("Retrieves a paginated list of comments for a post.");
        comments.MapPost("", AddComment).RequireAuthorization()
            .WithSummary("Add comment").WithDescription("Adds a comment to a post.");
        comments.MapDelete("/{commentId:guid}", DeleteComment).RequireAuthorization()
            .WithSummary("Delete comment").WithDescription("Deletes a comment from a post.");

        return app;
    }

    private static async Task<IResult> GetPost(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        Guid? currentUserId = TryGetUserId(user, out var userId)
            ? userId
            : null;

        var result = await mediator.Send(new GetPostQuery(id, currentUserId));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Post fetched successfully", result.Value));
    }

    private static async Task<IResult> ListPosts(
        IMediator mediator,
        Guid? authorId = null,
        string? cursor = null,
        int limit = 20)
    {
        var result = await mediator.Send(new ListPostsQuery(authorId, cursor, limit));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Posts fetched successfully", result.Value));
    }

    private static async Task<IResult> GetComments(
        Guid postId,
        IMediator mediator,
        string? cursor = null,
        int limit = 20)
    {
        var result = await mediator.Send(new GetCommentsQuery(postId, cursor, limit));

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Ok(
            ApiResponseFactory.Success("Comments fetched successfully", result.Value));
    }

    private static async Task<IResult> CreatePost(
        CreatePostCommand command,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(command with { AuthorId = userId });

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Created(
            $"/api/v1/posts/{result.Value}",
            ApiResponseFactory.Success(
                "Post created successfully",
                new { postId = result.Value }));
    }

    private static async Task<IResult> UpdatePost(
        Guid id,
        UpdatePostCommand command,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(command with
        {
            Id = id,
            RequestingUserId = userId
        });

        return result.ToApiResult();
    }

    private static async Task<IResult> DeletePost(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new DeletePostCommand(id, userId));

        return result.ToApiResult();
    }

    private static async Task<IResult> LikePost(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new LikePostCommand(id, userId));

        return result.ToApiResult();
    }

    private static async Task<IResult> UnlikePost(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new UnlikePostCommand(id, userId));

        return result.ToApiResult();
    }

    private static async Task<IResult> BookmarkPost(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new BookmarkPostCommand(id, userId));

        return result.ToApiResult();
    }

    private static async Task<IResult> RemoveBookmark(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new RemoveBookmarkCommand(id, userId));

        return result.ToApiResult();
    }

    private static async Task<IResult> AddComment(
        Guid postId,
        AddCommentCommand command,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(command with
        {
            PostId = postId,
            AuthorId = userId
        });

        if (result.IsFailure)
        {
            return result.ToApiResult();
        }

        return Results.Created(
            $"/api/v1/posts/{postId}/comments/{result.Value}",
            ApiResponseFactory.Success(
                "Comment created successfully",
                new { commentId = result.Value }));
    }

    private static async Task<IResult> DeleteComment(
        Guid postId,
        Guid commentId,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new DeleteCommentCommand(postId, commentId, userId));

        return result.ToApiResult();
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdValue =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out userId);
    }
}
