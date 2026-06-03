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

namespace CodeHive.Infrastructure.Data;

public class CodeHiveDbContext 
    : IdentityDbContext<ApplicationUser ,IdentityRole<Guid> ,Guid > 
{
    private readonly IMediator _mediator;
    private readonly IReadOnlyCollection<Assembly> _moduleAssemblies;

    public CodeHiveDbContext(
        DbContextOptions<CodeHiveDbContext> options,
        IMediator mediator,
        IReadOnlyCollection<Assembly> moduleAssemblies)
        : base(options)
    {
        _mediator = mediator;
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
        var Result = await base.SaveChangesAsync(ct);
        var Events = ChangeTracker.Entries<BaseEntity>()
                                    .SelectMany(e=>e.Entity.DomainEvents).ToList();
        
        foreach(var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
            foreach(var evt in Events)
            {
                await _mediator.Publish(evt , ct );
                
            }
        }

         return Result;
    }



    
}
