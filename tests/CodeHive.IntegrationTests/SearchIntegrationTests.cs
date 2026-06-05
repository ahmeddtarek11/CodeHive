using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CodeHive.Posts.Domain.Entities;
using CodeHive.Shared;
using CodeHive.Shared.Responses;

namespace CodeHive.IntegrationTests;

public sealed class SearchIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public SearchIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SearchPosts_ReturnsOk_WithResults()
    {
        // 1. Arrange: Create a post to search for
        var baseClient = _fixture.ApplicationFactory.CreateClient();
        var (client, _) = await TestAuthHelper.CreateAuthenticatedClientAsync(baseClient);

        var uniqueWord = $"SearchTest{Guid.NewGuid().ToString("N")[..8]}";
        
        var createResponse = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            Type = PostType.Blog,
            Title = $"Title with {uniqueWord}",
            Content = $"Content containing {uniqueWord}",
            Language = "markdown",
            Tags = new[] { "search" }
        });
        createResponse.EnsureSuccessStatusCode();

        // 2. Act: Search for the post
        var searchResponse = await client.GetAsync($"/api/v1/search/posts?q={uniqueWord}");

        // 3. Assert
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        
        var searchBody = await searchResponse.Content.ReadFromJsonAsync<ApiResponse<CursorPageResponse<JsonElement>>>();
        
        Assert.NotNull(searchBody);
        Assert.NotNull(searchBody!.Data);
        Assert.NotEmpty(searchBody.Data.Items);
        
        // Ensure the post is found
        var foundPost = searchBody.Data.Items.FirstOrDefault(p => p.ToString().Contains(uniqueWord));
        Assert.NotEqual(default, foundPost);
    }

    [Fact]
    public async Task SearchUsers_ReturnsOk_WithResults()
    {
        // 1. Arrange: Register a new user
        var client = _fixture.ApplicationFactory.CreateClient();
        var uniqueUsername = $"SearchUser{Guid.NewGuid().ToString("N")[..8]}";
        
        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = $"{uniqueUsername}@test.com",
            Username = uniqueUsername,
            Password = "P@ssw0rd123!",
            DisplayName = $"Display {uniqueUsername}"
        });
        registerResponse.EnsureSuccessStatusCode();

        // 2. Act: Search for the user
        var searchResponse = await client.GetAsync($"/api/v1/search/users?q={uniqueUsername}");

        // 3. Assert
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        
        var searchBody = await searchResponse.Content.ReadFromJsonAsync<ApiResponse<CursorPageResponse<JsonElement>>>();
        
        Assert.NotNull(searchBody);
        Assert.NotNull(searchBody!.Data);
        Assert.NotEmpty(searchBody.Data.Items);
        
        // Ensure the user is found
        var foundUser = searchBody.Data.Items.FirstOrDefault(u => u.ToString().Contains(uniqueUsername));
        Assert.NotEqual(default, foundUser);
    }

    private record CursorPageResponse<T>(IReadOnlyList<T> Items, string? NextCursor, bool HasMore);
}
