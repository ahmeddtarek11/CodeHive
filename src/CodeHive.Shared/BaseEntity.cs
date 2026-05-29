using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace CodeHive.Shared;

public abstract class BaseEntity
{
    public Guid Id { get; init; } =  Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    private readonly List<INotification> _domainEvents = [] ; 
    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();
    public void RasieDomainEvent(INotification e ) => _domainEvents.Add(e);
    public void ClearDomainEvents() => _domainEvents.Clear();
}



