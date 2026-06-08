using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared.Events;

public interface IUserRegisteredEvent
{
    Guid UserId {get;}
    string Email {get;}
    string Username {get;}
    string DisplayName {get;}
}
