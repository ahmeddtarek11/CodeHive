using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared;
using CodeHive.Shared.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CodeHive.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid> ,IHasDomainEvents
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    private readonly List<INotification> _domainEvents = [] ; 
    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();
    public void RasieDomainEvent(INotification e ) => _domainEvents.Add(e);
    public void ClearDomainEvents() => _domainEvents.Clear();

}
