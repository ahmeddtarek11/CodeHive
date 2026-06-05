using System.Net;
using System.Net.Http.Json;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared.Responses;

namespace CodeHive.IntegrationTests;

public sealed class PostIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public PostIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateUpdateDeletePost_Flow_WorksCorrectly()
    {
        // 1. Arrange
        var baseClient = _fixture.ApplicationFactory.CreateClient();
        var (client, _) = await TestAuthHelper.CreateAuthenticatedClientAsync(baseClient);

        // 2. Create Post
        var createResponse = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            Type = PostType.Blog,
            Title = "My first post",
            Content = "This is a test post.",
            Language = "markdown",
            Tags = new[] { "testing" }
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PostIdResponse>>();
        Assert.NotNull(createdBody);
        var postId = createdBody!.Data.PostId;
        Assert.NotEqual(Guid.Empty, postId);

        // 3. Update Post
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/posts/{postId}", new
        {
            Title = "Updated title",
            Content = "Updated content.",
            Language = "markdown",
            Tags = new[] { "testing", "updated" }
        });

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // 4. Delete Post
        var deleteResponse = await client.DeleteAsync($"/api/v1/posts/{postId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        
        // 5. Verify deletion (Get should return NotFound)
        var getResponse = await client.GetAsync($"/api/v1/posts/{postId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task PostInteractions_LikeUnlikeBookmark_WorksCorrectly()
    {
        // 1. Arrange
        var baseClient = _fixture.ApplicationFactory.CreateClient();
        var (client, _) = await TestAuthHelper.CreateAuthenticatedClientAsync(baseClient);

        // Create a post to interact with
        var createResponse = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            Type = PostType.Snippet,
            Title = "Interaction Test",
            Content = "print('Hello world')",
            Language = "python",
            Tags = new[] { "python" }
        });

        createResponse.EnsureSuccessStatusCode();
        var createdBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PostIdResponse>>();
        var postId = createdBody!.Data.PostId;

        // 2. Like Post
        var likeResponse = await client.PostAsync($"/api/v1/posts/{postId}/like", null);
        Assert.Equal(HttpStatusCode.NoContent, likeResponse.StatusCode);

        // 3. Unlike Post
        var unlikeResponse = await client.DeleteAsync($"/api/v1/posts/{postId}/like");
        Assert.Equal(HttpStatusCode.NoContent, unlikeResponse.StatusCode);

        // 4. Bookmark Post
        var bookmarkResponse = await client.PostAsync($"/api/v1/posts/{postId}/bookmark", null);
        Assert.Equal(HttpStatusCode.NoContent, bookmarkResponse.StatusCode);

        // 5. Remove Bookmark
        var removeBookmarkResponse = await client.DeleteAsync($"/api/v1/posts/{postId}/bookmark");
        Assert.Equal(HttpStatusCode.NoContent, removeBookmarkResponse.StatusCode);
    }

    private sealed record PostIdResponse(Guid PostId);
}
