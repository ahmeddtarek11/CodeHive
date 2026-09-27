using System.Security.Claims;
using CodeHive.Chat.Application.MarkMessageRead;
using CodeHive.Chat.Application.Queries.GetConversations;
using CodeHive.Chat.Application.Queries.GetMessages;
using CodeHive.Chat.Application.SendMessage;
using CodeHive.Chat.Application.StartConversation;
using CodeHive.Shared.Extensions;
using CodeHive.Shared.Responses;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CodeHive.Chat;

public static class ChatModule
{
    public static IServiceCollection AddChatModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ChatModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(ChatModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/conversations")
            .WithTags("Chat")
            .RequireAuthorization();

        // Start or retrieve a conversation
        group.MapPost("", StartConversation)
            .WithSummary("Start or get a conversation")
            .WithDescription("Initiates or gets an existing conversation with another user.");

        // List all conversations for the current user
        group.MapGet("", GetConversations)
            .WithSummary("Get conversations")
            .WithDescription("Retrieves a paginated list of conversations for the current user.");

        // Get paginated messages for a conversation
        group.MapGet("{conversationId:guid}/messages", GetMessages)
            .WithSummary("Get messages")
            .WithDescription("Retrieves paginated messages for a specific conversation.");

        // Send a message
        group.MapPost("{conversationId:guid}/messages", SendMessage)
            .WithSummary("Send message")
            .WithDescription("Sends a message to a specific conversation.");

        // Mark all messages in a conversation as read
        group.MapPatch("{conversationId:guid}/read", MarkMessagesRead)
            .WithSummary("Mark messages read")
            .WithDescription("Marks all messages in a conversation as read by the current user.");

        return app;
    }

    // ── Endpoint handlers ───────────────────────────────────────────────────

    private static async Task<IResult> StartConversation(
        StartConversationRequest req,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();
        
        var result = await mediator.Send(new StartConversationCommand(userId, req.TargetUserId));
        if (result.IsFailure) return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success(
                "Conversation ready",
                new { conversationId = result.Value }));
    }

    private static async Task<IResult> GetConversations(
        ClaimsPrincipal user,
        IMediator mediator,
        string? cursor = null,
        int limit = 20)
    {
        if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

        var result = await mediator.Send(new GetConversationsQuery(userId, cursor, limit));
        
        if (result.IsFailure) return result.ToApiResult();
        
        return Results.Ok(
            ApiResponseFactory.Success("Conversations fetched successfully", result.Value));
    }

    private static async Task<IResult> GetMessages(
        Guid conversationId,
        ClaimsPrincipal user,
        IMediator mediator,
        string? cursor = null,
        int limit = 30)
    {
        if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

        var result = await mediator.Send(new GetMessagesQuery(conversationId, userId, cursor, limit));

        if (result.IsFailure) return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success("Messages fetched successfully", result.Value));
    }

    private static async Task<IResult> SendMessage(
        Guid conversationId,
        SendMessageRequest req,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

        var result = await mediator.Send(new SendMessageCommand(conversationId, userId, req.Content));

        if (result.IsFailure) return result.ToApiResult();

        return Results.Ok(
            ApiResponseFactory.Success("Message sent successfully", result.Value));
    }

    private static async Task<IResult> MarkMessagesRead(
        Guid conversationId,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

        var result = await mediator.Send(new MarkMessagesReadCommand(conversationId, userId));

        return result.ToApiResult();
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdValue =
            user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out userId);
    }
}

public record StartConversationRequest(Guid TargetUserId);
public record SendMessageRequest(string Content);
