using System.Net;
using System.Net.Http.Json;
using CodeHive.Shared.Responses;

namespace CodeHive.IntegrationTests;

public sealed class UserAuthenticationFlowTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public UserAuthenticationFlowTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RegisterThenLogin_ReturnsCreatedUserIdAndTokens()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"integration-{suffix}@test.com";
        var username = $"integrationuser-{suffix}";
        var password = "P@ssw0rd123!";

        var client = _fixture.ApplicationFactory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Username = username,
            Password = password,
            DisplayName = $"Integration {suffix}"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerBody = await registerResponse.Content
            .ReadFromJsonAsync<ApiResponse<RegistrationResponse>>();

        Assert.NotNull(registerBody);
        Assert.NotEqual(Guid.Empty, registerBody!.Data.UserId);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content
            .ReadFromJsonAsync<ApiResponse<AuthTokensResponse>>();

        Assert.NotNull(loginBody);
        Assert.False(string.IsNullOrWhiteSpace(loginBody!.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(loginBody.Data.RefreshToken));
        Assert.NotEqual(default, loginBody.Data.AccessTokenExpiry);
    }

    private sealed record RegistrationResponse(Guid UserId);

    private sealed record AuthTokensResponse(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiry);
}
