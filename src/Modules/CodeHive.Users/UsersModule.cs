using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using FluentValidation;
using CodeHive.Users.Domain;


namespace CodeHive.Users;

public static class UsersModule
{

    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UsersModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(UsersModule).Assembly);
        services.AddScoped<IExternalAuthService, ExternalAuthService>();
        return services;
    }





}
