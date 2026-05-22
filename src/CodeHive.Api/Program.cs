

using MediatR;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


//serilog 
builder.Host.UseSerilog();


//openAI
builder.Services.AddOpenApi();

// infrastructure DbContext, Identity , JWT , Redis , MassTransit 
//builder.Services.AddInfrastructure(builder.Configuration);


// modules
// builder.Services.AddUserModule();
// builder.Services.AddPostsModule();
// builder.Services.AddFeedModule();
// builder.Services.AddNotificationsModule();
// builder.Services.AddSearchModules();

// MediatR pipeline 
//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior));
//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

//Api concerns 
builder.Services.AddOpenApi();
//builder.Services.AddHealthChecks().AddNpgSql().AddRedis();

var app = builder.Build();

app.UseExceptionHandler(); // global error handler
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

//app.MapUsersEndpoints();
//app.MapPostsEndpoints();
//app.MapFeedEndpoints();
//app.MapNotificationsEndpoints();
//app.MapSearchEndpoints();
app.MapHealthChecks("/health");
app.MapScalarApiReference();


app.Run();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())




