using CodeHive.Shared;

namespace CodeHive.Posts.Domain.Errors;

public static class PostErrors
{

    public static readonly Error NotFound = Error.NotFound("Post");
    public static readonly Error NotAuthor = Error.Forbidden();
    public static readonly Error AlreadyLiked = new("Post.AlreadyLiked", "You have already liked this post.");
    public static readonly Error NotLiked = new("Post.NotLiked","You have not liked this post.");    

}
