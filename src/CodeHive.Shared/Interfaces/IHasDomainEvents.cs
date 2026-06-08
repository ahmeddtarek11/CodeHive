using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace CodeHive.Shared.Interfaces;

public interface IHasDomainEvents
{
   
    public IReadOnlyList<INotification> DomainEvents {get;}
    public void RasieDomainEvent(INotification e ) ;
    public void ClearDomainEvents() ;
}
