using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Users.Domain.Data.Entities;

public sealed class Follow 
{
    public Guid     FollowerId { get; set; }
    public Guid     FolloweeId { get; set; }
    public DateTime FollowedAt { get; init; } = DateTime.UtcNow;
}
