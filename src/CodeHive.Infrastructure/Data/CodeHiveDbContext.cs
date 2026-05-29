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
    public CodeHiveDbContext(DbContextOptions<CodeHiveDbContext> options , IMediator mediator)
        : base(options)
    {
        _mediator  = mediator ;
    }

    // public DbSet<post>
    // 
    //   ............. dbsets


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Each module registers its EF configs automatically from its assembly.
        // You will add more lines here as you create modules.
        builder.ApplyConfigurationsFromAssembly(typeof(CodeHiveDbContext).Assembly);
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
