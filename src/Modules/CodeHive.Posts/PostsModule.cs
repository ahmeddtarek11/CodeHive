using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CodeHive.Posts;

public static class PostsModule
{
    public static IServiceCollection AddPostsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PostsModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(PostsModule).Assembly);
        return services;
    }
}
