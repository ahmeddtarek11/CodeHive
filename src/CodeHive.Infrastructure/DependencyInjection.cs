using System.Reflection;
using System.Text;
using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Shared.Interfaces.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CodeHive.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] moduleAssemblies)
    {
        services.AddSingleton<IReadOnlyCollection<Assembly>>(moduleAssemblies);

        services
            .AddPersistence(configuration)
            .AddIdentityServices()
            .AddJwtAuthentication(configuration);

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

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            });

            var OauthSettings = configuration.GetSection("OAuth").Get<OAuthSettings>();
            services.Configure<OAuthSettings>(configuration.GetSection("OAuth"));


            services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = OauthSettings!.Google.ClientId;
                options.ClientSecret = OauthSettings!.Google.ClientSecret;
                options.CallbackPath = "/api/v1/auth/oauth/google/callback";

                 options.SignInScheme  = IdentityConstants.ExternalScheme;


            }).AddGitHub(opts =>
            {
                opts.ClientId      = OauthSettings!.GitHub.ClientId;
                opts.ClientSecret  = OauthSettings.GitHub.ClientSecret;
                opts.CallbackPath  = "/api/v1/auth/oauth/github/callback";
                opts.SignInScheme  = IdentityConstants.ExternalScheme;

                opts.Scope.Add("user:email");
            });



        services.AddAuthorization();
        return services;
    }
}
