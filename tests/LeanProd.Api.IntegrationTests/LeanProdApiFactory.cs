using System.Security.Cryptography;
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
    public static string AdminPassword { get; } = NewPassword();
    private static readonly string DatabasePassword = NewPassword();

    private readonly MsSqlContainer businessDatabase = new MsSqlBuilder()
        .WithPassword(DatabasePassword)
        .Build();
    private readonly string localSettingsPath = Path.Combine(
        Path.GetTempPath(), "LeanProd.IntegrationTests", Guid.NewGuid().ToString("N"));
    private string InternalDatabasePath => Path.Combine(localSettingsPath, "leanprod-internal.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Database:Provider"] = "SqlServer",
                ["Database:ApplyMigrationsOnStartup"] = "true",
                ["Database:LocalSettingsPath"] = localSettingsPath,
                ["ConnectionStrings:DefaultConnection"] = businessDatabase.GetConnectionString(),
                ["InternalDatabase:Path"] = InternalDatabasePath,
                ["InternalOrganization:LegalName"] = "LeanProd Integration Tests",
                ["InternalOrganization:CountryCode"] = "BY",
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

    public Task InitializeAsync() => businessDatabase.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await businessDatabase.DisposeAsync();
        if (Directory.Exists(localSettingsPath)) Directory.Delete(localSettingsPath, recursive: true);
    }

    public async Task AssertMigrationsAppliedAsync()
    {
        using var scope = Services.CreateScope();
        var business = scope.ServiceProvider.GetRequiredService<LeanProdDbContext>();
        Assert.Empty(await business.Database.GetPendingMigrationsAsync());
        Assert.NotEmpty(await business.Database.GetAppliedMigrationsAsync());
        var identity = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        Assert.Empty(await identity.Database.GetPendingMigrationsAsync());
        Assert.NotEmpty(await identity.Database.GetAppliedMigrationsAsync());
    }

    private static string NewPassword() => $"Aa1!{Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}";
}
