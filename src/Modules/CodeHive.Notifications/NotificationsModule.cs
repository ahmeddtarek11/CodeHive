using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CodeHive.Notifications.Application.Commands.MarkRead;
using CodeHive.Notifications.Application.Queries;
using CodeHive.Notifications.Application.Queries.GetUnreadCount;
using CodeHive.Shared.Extensions;
using CodeHive.Shared.Responses;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CodeHive.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NotificationsModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(NotificationsModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        notifications.MapGet("",GetNotifications);
        notifications.MapGet("/unread-count", GetUnreadCount);
        notifications.MapPatch("/{id:guid}/read", MarkNotificationRead);
        notifications.MapPatch("/read-all",   MarkAllNotificationsRead);

        return app;
    }

    // GET /api/v1/notifications
    private static async Task<IResult> GetNotifications(
        ClaimsPrincipal user,
        IMediator mediator,
        string? cursor = null,
        int limit = 20)
    {
        if (!TryGetUserId(user, out var userId))
            return Results.Unauthorized();

        var result = await mediator.Send(new GetNotificationsQuery(userId, cursor, limit));

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.Ok(ApiResponseFactory.Success("Notifications fetched successfully", result.Value));
    }

    // GET /api/v1/notifications/unread-count
    private static async Task<IResult> GetUnreadCount(
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
            return Results.Unauthorized();

        var result = await mediator.Send(new GetUnreadCountQuery(userId));

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.Ok(ApiResponseFactory.Success("Unread count fetched successfully", result.Value));
    }

    // PATCH /api/v1/notifications/{id}/read
    private static async Task<IResult> MarkNotificationRead(
        Guid id,
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
            return Results.Unauthorized();

        var result = await mediator.Send(new MarkNotificationReadCommand(id, userId));

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.NoContent();
    }

    // PATCH /api/v1/notifications/read-all
    private static async Task<IResult> MarkAllNotificationsRead(
        ClaimsPrincipal user,
        IMediator mediator)
    {
        if (!TryGetUserId(user, out var userId))
            return Results.Unauthorized();

        var result = await mediator.Send(new MarkAllNotificationsReadCommand(userId));

        if (result.IsFailure)
            return result.ToApiResult();

        return Results.NoContent();
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var value =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out userId);
    }
}
