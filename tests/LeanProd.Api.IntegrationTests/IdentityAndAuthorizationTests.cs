using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LeanProd.Api.IntegrationTests;

public sealed class IdentityAndAuthorizationTests(LeanProdApiFactory factory)
    : IClassFixture<LeanProdApiFactory>
{
    [Fact]
    public async Task Health_and_migrations_are_ready()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
        await factory.AssertMigrationsAppliedAsync();
    }

    [Fact]
    public async Task Protected_endpoint_returns_problem_details_without_token()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/platform/workspace-access");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Admin_can_access_admin_policy_but_new_viewer_is_forbidden()
    {
        using var admin = factory.CreateClient();
        var adminToken = await Login(admin, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        Assert.Equal(HttpStatusCode.NoContent,
            (await admin.GetAsync("/api/platform/administration-access")).StatusCode);

        using var viewer = factory.CreateClient();
        var email = $"viewer-{Guid.NewGuid():N}@leanprod.test";
        var registration = await viewer.PostAsJsonAsync("/api/identity/register", new
        {
            email,
            displayName = "Integration Viewer",
            password = "Viewer@Test123!"
        });
        registration.EnsureSuccessStatusCode();
        var viewerUser = await registration.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("Viewer", viewerUser.GetProperty("roles").EnumerateArray().Select(x => x.GetString()));
        viewer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", viewerUser.GetProperty("accessToken").GetString());
        Assert.Equal(HttpStatusCode.Forbidden,
            (await viewer.GetAsync("/api/platform/administration-access")).StatusCode);
    }

    [Fact]
    public async Task Refresh_token_is_rotated_and_logout_revokes_session()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        });
        var firstToken = await Login(client, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword);
        var refresh = await client.PostAsJsonAsync("/api/identity/refresh", new { });
        refresh.EnsureSuccessStatusCode();
        var refreshed = await refresh.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEqual(firstToken, refreshed.GetProperty("accessToken").GetString());

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync("/api/identity/logout", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/identity/refresh", new { })).StatusCode);
    }

    private static async Task<string> Login(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/identity/login", new
        {
            email,
            password,
            rememberMe = false
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }
}
