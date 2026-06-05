using CodeHive.Shared;

namespace CodeHive.Posts.Domain.Entities;

public sealed class Comment : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
