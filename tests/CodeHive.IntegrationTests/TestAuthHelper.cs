using System.Net.Http.Headers;
using System.Net.Http.Json;
using CodeHive.Shared.Responses;

namespace CodeHive.IntegrationTests;

public static class TestAuthHelper
{
    public static async Task<(HttpClient Client, Guid UserId)> CreateAuthenticatedClientAsync(HttpClient client)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"user-{suffix}@test.com";
        var username = $"user-{suffix}";
        var password = "P@ssw0rd123!";

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Username = username,
            Password = password,
            DisplayName = $"Test User {suffix}"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<RegistrationResponse>>();
        var userId = registerBody!.Data.UserId;

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = email,
            Password = password
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthTokensResponse>>();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.Data.AccessToken);

        return (client, userId);
    }

    private sealed record RegistrationResponse(Guid UserId);
    private sealed record AuthTokensResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiry);
}
