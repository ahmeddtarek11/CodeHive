
using CodeHive.Api.Middlewares;
using CodeHive.Infrastructure;
using CodeHive.Shared.Behaviors;
using CodeHive.Users;
using CodeHive.Users.Domain.Data.Config;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
       .WriteTo.File("logs/CodeHive.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));

// infrastructure: JWT auth + token service (DbContext, Identity, Redis, MassTransit later)
builder.Services.AddInfrastructure(builder.Configuration, typeof(FollowConfigurations).Assembly);

builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();
// modules
 builder.Services.AddUsersModule();
// builder.Services.AddPostsModule();
// builder.Services.AddFeedModule();
// builder.Services.AddNotificationsModule();
// builder.Services.AddSearchModules();

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



var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
//app.UseRateLimiter(); // TODO: add AddRateLimiter() when rate limiting is ready

app.MapUsersEndpoints();
//app.MapPostsEndpoints();
//app.MapFeedEndpoints();
//app.MapNotificationsEndpoints();
//app.MapSearchEndpoints();
app.MapHealthChecks("/health");
app.MapOpenApi();
app.MapScalarApiReference();

app.Run();

