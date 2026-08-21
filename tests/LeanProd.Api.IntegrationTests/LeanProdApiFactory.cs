using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace LeanProd.Api.IntegrationTests;

public sealed class LeanProdApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "integration.admin@leanprod.test";
    public const string AdminPassword = "Integration@Test123!";

    private readonly MsSqlContainer database = new MsSqlBuilder()
        .WithPassword("Integration@Test123!")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Database:Provider"] = "SqlServer",
                ["Database:ApplyMigrationsOnStartup"] = "true",
                ["ConnectionStrings:DefaultConnection"] = database.GetConnectionString(),
                ["Jwt:Issuer"] = "LeanProd.Api.IntegrationTests",
                ["Jwt:Audience"] = "LeanProd.Web.IntegrationTests",
                ["Jwt:SigningKey"] = new string('I', 64),
                ["Jwt:AccessTokenMinutes"] = "10",
                ["Jwt:RefreshTokenDays"] = "1",
                ["BootstrapAdmin:Email"] = AdminEmail,
                ["BootstrapAdmin:Password"] = AdminPassword,
                ["Identity:AllowPublicRegistration"] = "true",
                ["Https:UseRedirection"] = "false"
            }));
    }

    public async Task InitializeAsync() => await database.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await database.DisposeAsync();
    }

    public async Task AssertMigrationsAppliedAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LeanProdDbContext>();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.NotEmpty(await db.Database.GetAppliedMigrationsAsync());
    }
}
