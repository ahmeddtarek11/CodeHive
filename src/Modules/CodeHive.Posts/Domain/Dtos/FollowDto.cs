using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Posts.Domain.Dtos;

public sealed class Follow 
{
    public Guid     FollowerId { get; set; }
    public Guid     FolloweeId { get; set; }
    public DateTime FollowedAt { get; init; } = DateTime.UtcNow;
}
