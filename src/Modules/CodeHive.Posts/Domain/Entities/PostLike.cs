using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Posts.Domain.Entities;

public class PostLike
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTime LikedAt { get; init; } = DateTime.UtcNow;
}
