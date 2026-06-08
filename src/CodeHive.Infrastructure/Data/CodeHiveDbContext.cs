using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Infrastructure.Identity;
using CodeHive.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CodeHive.Infrastructure.Outbox;
using System.Text.Json;
using CodeHive.Shared.Interfaces;

namespace CodeHive.Infrastructure.Data;

public class CodeHiveDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{

    private readonly IReadOnlyCollection<Assembly> _moduleAssemblies;

    public CodeHiveDbContext(
        DbContextOptions<CodeHiveDbContext> options,
        IReadOnlyCollection<Assembly> moduleAssemblies)
        : base(options)
    {

        _moduleAssemblies = moduleAssemblies;
    }



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(CodeHiveDbContext).Assembly);

        foreach (var assembly in _moduleAssemblies)
        {
            builder.ApplyConfigurationsFromAssembly(assembly);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {

        var Events = ChangeTracker.Entries<IHasDomainEvents>()
                                    .SelectMany(e => e.Entity.DomainEvents).ToList();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();

        }

        foreach (var evt in Events)
        {
            Set<OutboxMessage>().Add(new OutboxMessage
            {
                EventType = evt.GetType().AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(evt, evt.GetType())
            });
        }

        return await base.SaveChangesAsync(ct); ;
    }




}
