
using System.Reflection;
using CodeHive.Api.Middlewares;
using CodeHive.Feed;
using CodeHive.Infrastructure;
using CodeHive.Posts;
using CodeHive.Posts.Domain.Config;
using CodeHive.Shared.Behaviors;
using CodeHive.Search;
using CodeHive.Users;
using CodeHive.Users.Domain.Data.Config;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Scalar.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
       .WriteTo.File("logs/CodeHive.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));


// infrastructure: JWT auth + token service (DbContext, Identity, Redis, MassTransit later)
builder.Services.AddInfrastructure(builder.Configuration, 
    typeof(FollowConfigurations).Assembly ,
    typeof(PostConfiguration).Assembly
    );

builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();
// modules
 builder.Services.AddUsersModule();
builder.Services.AddPostsModule();
builder.Services.AddFeedModule();
// builder.Services.AddNotificationsModule();
builder.Services.AddSearchModules();

// MediatR — scan module assemblies here as they are built
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// API concerns
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddRateLimiter(options =>
{
   options.AddFixedWindowLimiter("auth" ,cfg =>
   {
       cfg.PermitLimit = 5 ;
       cfg.QueueLimit = 0;
       cfg.Window = TimeSpan.FromMinutes(5);
   });
   options.RejectionStatusCode = 429 ;
});



var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();


app.MapUsersEndpoints();
app.MapPostsEndpoints();
app.MapFeedEndpoints();
//app.MapNotificationsEndpoints();
app.MapSearchEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});
app.MapOpenApi();
app.MapScalarApiReference();

app.Run();

static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var payload = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.ToString(),
        entries = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => new
            {
                status = entry.Value.Status.ToString()
            })
    };

    return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
}











public partial class Program
{
    
}