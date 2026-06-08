using System.Reflection;
using System.Text;
using CodeHive.Infrastructure.Caching;
using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Infrastructure.Outbox;
using CodeHive.Shared.Interfaces.Identity;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace CodeHive.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] moduleAssemblies)
    {
        services.AddSingleton<IReadOnlyCollection<Assembly>>(moduleAssemblies);
        services.AddHostedService<OutboxProcessor>();

        services
            .AddPersistence(configuration)
            .AddIdentityServices()
            .AddJwtAuthentication(configuration);


        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumers(typeof(DependencyInjection).Assembly);

            cfg.UsingRabbitMq((ctx, rabbitCfg)=>
            {
                rabbitCfg.Host(configuration.GetConnectionString("RabbitMq"));

                   // MassTransit auto-creates exchanges and queues based on consumer types.
                  // This line tells it to do that for all registered consumers.
                    rabbitCfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        services.AddSingleton<ICacheService , RedisCacheService>();


        return services;


    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CodeHiveDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddHealthChecks()
            .AddDbContextCheck<CodeHiveDbContext>();

        return services;
    }

    private static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        
        services
            .AddIdentityCore<ApplicationUser>(opts =>
            {
                opts.Password.RequireDigit = true;
                opts.Password.RequiredLength = 4;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireLowercase =false;
                opts.User.RequireUniqueEmail = true;
                
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<CodeHiveDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<ITokenService, TokenService>();


        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"Missing '{JwtSettings.SectionName}' configuration section.");

        var oauthSettings = configuration.GetSection("OAuth").Get<OAuthSettings>()
            ?? throw new InvalidOperationException(
                "Missing 'OAuth' configuration section.");

        services.Configure<OAuthSettings>(configuration.GetSection("OAuth"));

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        authBuilder
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddCookie(IdentityConstants.ExternalScheme)
            .AddGoogle(options =>
            {
                options.ClientId = oauthSettings.Google.ClientId;
                options.ClientSecret = oauthSettings.Google.ClientSecret;
                options.CallbackPath = "/signin-google";
                options.SignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddGitHub(options =>
            {
                options.ClientId = oauthSettings.GitHub.ClientId;
                options.ClientSecret = oauthSettings.GitHub.ClientSecret;
                options.CallbackPath = "/signin-github";
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Scope.Add("user:email");
            });



        services.AddAuthorization();
        return services;
    }
}
