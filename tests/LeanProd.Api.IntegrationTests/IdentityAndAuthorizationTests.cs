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

    [Fact]
    public async Task Only_admin_can_list_and_create_users()
    {
        using var admin = factory.CreateClient();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", await Login(admin, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword));
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/users")).StatusCode);

        var email = $"managed-{Guid.NewGuid():N}@leanprod.test";
        var created = await admin.PostAsJsonAsync("/api/users", new
        {
            email, displayName = "Managed Viewer", temporaryPassword = "Managed@Test123!",
            roles = new[] { "Viewer" }
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        using var viewer = factory.CreateClient();
        viewer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", await Login(viewer, email, "Managed@Test123!"));
        Assert.Equal(HttpStatusCode.Forbidden, (await viewer.GetAsync("/api/users")).StatusCode);
    }

    [Fact]
    public async Task Administrator_cannot_deactivate_self_or_remove_last_administrator_role()
    {
        using var admin = factory.CreateClient();
        var token = await Login(admin, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await (await admin.GetAsync("/api/identity/me")).Content.ReadFromJsonAsync<JsonElement>();
        var id = me.GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Conflict,
            (await admin.PostAsJsonAsync($"/api/users/{id}/deactivate", new { })).StatusCode);

        var details = await (await admin.GetAsync($"/api/users/{id}")).Content.ReadFromJsonAsync<JsonElement>();
        var removeRole = await admin.PutAsJsonAsync($"/api/users/{id}/roles", new
        {
            roles = new[] { "Viewer" },
            concurrencyStamp = details.GetProperty("concurrencyStamp").GetString()
        });
        Assert.Equal(HttpStatusCode.Conflict, removeRole.StatusCode);
    }

    [Fact]
    public async Task Deactivation_revokes_refresh_session_and_blocks_login()
    {
        using var admin = factory.CreateClient();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", await Login(admin, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword));
        var email = $"inactive-{Guid.NewGuid():N}@leanprod.test";
        var create = await admin.PostAsJsonAsync("/api/users", new
        {
            email, displayName = "Inactive User", temporaryPassword = "Inactive@Test123!",
            roles = new[] { "Viewer" }
        });
        var user = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = user.GetProperty("id").GetGuid();

        using var session = factory.CreateClient(new WebApplicationFactoryClientOptions
        { HandleCookies = true, BaseAddress = new Uri("https://localhost") });
        await Login(session, email, "Inactive@Test123!");
        (await admin.PostAsJsonAsync($"/api/users/{id}/deactivate", new { })).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await session.PostAsJsonAsync("/api/identity/refresh", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await session.PostAsJsonAsync("/api/identity/login", new
                { email, password = "Inactive@Test123!", rememberMe = false })).StatusCode);
    }

    [Fact]
    public async Task User_interface_language_is_saved_and_returned_on_next_login()
    {
        using var client = factory.CreateClient();
        var token = await Login(client, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PutAsJsonAsync("/api/identity/preferences", new { preferredLanguage = "en" })).StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var login = await client.PostAsJsonAsync("/api/identity/login", new
        {
            email = LeanProdApiFactory.AdminEmail,
            password = LeanProdApiFactory.AdminPassword,
            rememberMe = false
        });
        var user = await login.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("en", user.GetProperty("preferredLanguage").GetString());
    }

    [Fact]
    public async Task Administrator_can_create_department_and_storage_with_multiple_types()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", await Login(client, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword));
        var code = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        var departmentResponse = await client.PostAsJsonAsync("/api/departments", new
        {
            code, name = "Integration department", description = "Test", parentDepartmentId = (Guid?)null
        });
        Assert.Equal(HttpStatusCode.Created, departmentResponse.StatusCode);
        var department = await departmentResponse.Content.ReadFromJsonAsync<JsonElement>();

        var kinds = await (await client.GetAsync("/api/storage-locations/kinds")).Content.ReadFromJsonAsync<JsonElement>();
        var types = await (await client.GetAsync("/api/storage-locations/types")).Content.ReadFromJsonAsync<JsonElement>();
        var kindId = kinds[0].GetProperty("id").GetGuid();
        var typeIds = types.EnumerateArray().Take(2).Select(x => x.GetProperty("id").GetGuid()).ToArray();
        var storageResponse = await client.PostAsJsonAsync("/api/storage-locations", new
        {
            code = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant(), name = "Integration storage",
            departmentId = department.GetProperty("id").GetGuid(), kindId,
            parentStorageLocationId = (Guid?)null, typeIds, address = "Test address"
        });
        Assert.Equal(HttpStatusCode.Created, storageResponse.StatusCode);
        var storage = await storageResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, storage.GetProperty("typeIds").GetArrayLength());

        var me = await (await client.GetAsync("/api/identity/me")).Content.ReadFromJsonAsync<JsonElement>();
        var adminDetails = await (await client.GetAsync($"/api/users/{me.GetProperty("id").GetGuid()}")).Content.ReadFromJsonAsync<JsonElement>();
        var updateUser = await client.PutAsJsonAsync($"/api/users/{me.GetProperty("id").GetGuid()}", new
        {
            email = LeanProdApiFactory.AdminEmail,
            displayName = adminDetails.GetProperty("displayName").GetString(),
            preferredLanguage = "be",
            defaultDepartmentId = (Guid?)null,
            defaultStorageLocationId = storage.GetProperty("id").GetGuid(),
            concurrencyStamp = adminDetails.GetProperty("concurrencyStamp").GetString()
        });
        updateUser.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Conflict,
            (await client.PostAsJsonAsync($"/api/departments/{department.GetProperty("id").GetGuid()}/deactivate", new { })).StatusCode);
    }

    [Fact]
    public async Task Storage_location_requires_at_least_one_type()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", await Login(client, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword));
        var departments = await (await client.GetAsync("/api/departments/options")).Content.ReadFromJsonAsync<JsonElement>();
        if (departments.GetArrayLength() == 0) return;
        var kinds = await (await client.GetAsync("/api/storage-locations/kinds")).Content.ReadFromJsonAsync<JsonElement>();
        var response = await client.PostAsJsonAsync("/api/storage-locations", new
        {
            code = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant(), name = "Invalid storage",
            departmentId = departments[0].GetProperty("id").GetGuid(), kindId = kinds[0].GetProperty("id").GetGuid(),
            typeIds = Array.Empty<Guid>()
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Administrator_can_add_catalog_units_and_define_one_bidirectional_conversion()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", await Login(client, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword));

        var kilogramResponse = await client.PostAsJsonAsync("/api/unit-of-measures", new
        {
            catalogCode = "166", quantityType = "Mass", decimalPlaces = 3,
            languageCode = "be", localizedName = "кілаграм"
        });
        Assert.Equal(HttpStatusCode.Created, kilogramResponse.StatusCode);
        var kilogram = await kilogramResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("KGM", kilogram.GetProperty("letterCode").GetString());
        Assert.Equal("кілаграм", kilogram.GetProperty("displayName").GetString());

        var tonneResponse = await client.PostAsJsonAsync("/api/unit-of-measures", new
        {
            catalogCode = "168", quantityType = "Mass", decimalPlaces = 3,
            languageCode = "be", localizedName = "тона"
        });
        Assert.Equal(HttpStatusCode.Created, tonneResponse.StatusCode);
        var tonne = await tonneResponse.Content.ReadFromJsonAsync<JsonElement>();

        var conversionResponse = await client.PostAsJsonAsync("/api/unit-of-measures/conversions", new
        {
            fromUnitId = tonne.GetProperty("id").GetGuid(),
            toUnitId = kilogram.GetProperty("id").GetGuid(), multiplier = 1000m, offset = 0m
        });
        Assert.Equal(HttpStatusCode.OK, conversionResponse.StatusCode);

        var calculation = await client.PostAsJsonAsync("/api/unit-of-measures/conversions/calculate", new
        {
            fromUnitId = tonne.GetProperty("id").GetGuid(),
            toUnitId = kilogram.GetProperty("id").GetGuid(), value = 2m
        });
        calculation.EnsureSuccessStatusCode();
        Assert.Equal(2000m, (await calculation.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("result").GetDecimal());

        var reverseDuplicate = await client.PostAsJsonAsync("/api/unit-of-measures/conversions", new
        {
            fromUnitId = kilogram.GetProperty("id").GetGuid(),
            toUnitId = tonne.GetProperty("id").GetGuid(), multiplier = 0.001m, offset = 0m
        });
        Assert.Equal(HttpStatusCode.Conflict, reverseDuplicate.StatusCode);
    }

    [Fact]
    public async Task Equipment_allows_missing_inventory_numbers_and_keeps_state_history()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", await Login(client, LeanProdApiFactory.AdminEmail, LeanProdApiFactory.AdminPassword));
        var departmentResponse = await client.PostAsJsonAsync("/api/departments", new
        {
            code = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant(),
            name = "Equipment integration department", parentDepartmentId = (Guid?)null
        });
        departmentResponse.EnsureSuccessStatusCode();
        var department = await departmentResponse.Content.ReadFromJsonAsync<JsonElement>();
        var departmentId = department.GetProperty("id").GetGuid();

        async Task<JsonElement> CreateEquipment(string name, string? inventoryNumber)
        {
            var response = await client.PostAsJsonAsync("/api/equipment", new
            {
                name, inventoryNumber, equipmentTypeId = (Guid?)null, departmentId,
                parentEquipmentId = (Guid?)null
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.Content.ReadFromJsonAsync<JsonElement>();
        }

        var machine = await CreateEquipment("Machine", null);
        await CreateEquipment("Machine assembly", null);
        await CreateEquipment("Inventoried machine", "EQ-001");
        var duplicate = await client.PostAsJsonAsync("/api/equipment", new
        {
            name = "Duplicate inventory", inventoryNumber = "EQ-001", equipmentTypeId = (Guid?)null,
            departmentId, parentEquipmentId = (Guid?)null
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var equipmentId = machine.GetProperty("id").GetGuid();
        var firstStart = DateTime.UtcNow.AddHours(-2);
        var secondStart = DateTime.UtcNow.AddHours(-1);
        (await client.PostAsJsonAsync($"/api/equipment/{equipmentId}/states", new
            { state = "Operational", startedAtUtc = firstStart, comment = "Started" })).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync($"/api/equipment/{equipmentId}/states", new
            { state = "Repair", startedAtUtc = secondStart, comment = "Bearing" })).EnsureSuccessStatusCode();
        var plannedStart = DateTime.UtcNow.AddDays(1);
        var plannedEnd = plannedStart.AddHours(2);
        var plannedResponse = await client.PostAsJsonAsync($"/api/equipment/{equipmentId}/states", new
            { state = "Maintenance", startedAtUtc = plannedStart, endedAtUtc = plannedEnd, comment = "Planned service" });
        plannedResponse.EnsureSuccessStatusCode();
        var planned = await plannedResponse.Content.ReadFromJsonAsync<JsonElement>();
        var editedEnd = plannedStart.AddHours(3);
        var editResponse = await client.PutAsJsonAsync($"/api/equipment/{equipmentId}/states/{planned.GetProperty("id").GetGuid()}", new
        {
            state = "Maintenance", startedAtUtc = plannedStart, endedAtUtc = editedEnd,
            comment = "Updated planned service", rowVersion = planned.GetProperty("rowVersion").GetString()
        });
        editResponse.EnsureSuccessStatusCode();

        var history = await (await client.GetAsync($"/api/equipment/{equipmentId}/states"))
            .Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, history.GetArrayLength());
        Assert.Equal("Operational", history[0].GetProperty("state").GetString());
        Assert.Equal("Repair", history[1].GetProperty("state").GetString());
        Assert.Equal("Maintenance", history[2].GetProperty("state").GetString());
        Assert.Equal("Updated planned service", history[2].GetProperty("comment").GetString());
        Assert.NotEqual(JsonValueKind.Null, history[2].GetProperty("endedAtUtc").ValueKind);
        Assert.False(history[1].GetProperty("endedAtUtc").ValueKind == JsonValueKind.Null);
        var details = await (await client.GetAsync($"/api/equipment/{equipmentId}"))
            .Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Repair", details.GetProperty("currentState").GetString());
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
