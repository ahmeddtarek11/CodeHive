using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Posts.Domain.Entities;

public class Bookmark
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTime BookmarkedAt { get; init; } = DateTime.UtcNow;
}
