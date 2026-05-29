using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace CodeHive.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    ILogger<ExceptionHandlingMiddleware> logger) : IExceptionHandler
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false; 
        }

        if (exception is ValidationException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error on {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage)
                        .Distinct()
                        .ToArray());

            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    errors
                },
                cancellationToken);

            return true;
        }

        _logger.LogError(
            exception,
            "Unhandled exception on {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                message = "An unexpected error occurred."
            },
            cancellationToken);

        return true;
    }
}
