using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared;

namespace CodeHive.Posts.Domain.Entities;

public class Post : BaseEntity
{
    public Guid      AuthorId  { get; set; }
    public PostType  Type      { get; set; }
    public string?   Title     { get; set; }     
    public string    Content   { get; set; } = string.Empty;
    public string?   Language  { get; set; }     // Snippet only
    public bool      IsDeleted { get; set; } = false;
}
