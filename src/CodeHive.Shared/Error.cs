using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared;

public record Error(string Code , string Description )
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error NotFound(string entity) =>
        new($"{entity}.NotFound", $"{entity} was not found.");  

    public static Error Conflict(string entity) =>
        new($"{entity}.AlreadyExists" , $"{entity} already exists");

    public static Error Unauthorized() =>
        new("Auth.Unauthorized", "You are not authorized.");

    public static Error Forbidden() =>
        new("Auth.Forbidden", "You do not have permission to do this.");

    

}
