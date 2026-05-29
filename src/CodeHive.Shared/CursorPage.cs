using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared;

public record CursorPage<T>(IReadOnlyList<T> Items , string? NextCursor , bool HasMore )
{
    
}
