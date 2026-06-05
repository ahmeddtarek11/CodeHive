using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Posts.Domain.Entities;

public class Tag
{
    private string _name = string.Empty;

    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }
}
