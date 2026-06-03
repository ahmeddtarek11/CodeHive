using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Users.Domain.Data.Config;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CodeHive.Users.Tests;

internal static class TestHelpers
{
    public static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = Substitute.For<IPasswordHasher<ApplicationUser>>();
        var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();
        var lookupNormalizer = Substitute.For<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = Substitute.For<IServiceProvider>();
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>();

        return Substitute.For<UserManager<ApplicationUser>>(
            store,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            lookupNormalizer,
            errors,
            services,
            logger);
    }

    public static CodeHiveDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CodeHiveDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var mediator = Substitute.For<IMediator>();
        var moduleAssemblies = new[] { typeof(RefreshTokenConfiguration).Assembly };

        return new CodeHiveDbContext(options, mediator, moduleAssemblies);
    }
}
